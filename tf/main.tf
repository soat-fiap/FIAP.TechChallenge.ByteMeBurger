##############################
# NAMESPACE
##############################

resource "kubernetes_namespace" "orders" {
  metadata {
    name = "fiap-orders"
  }
}

locals {
  connection_string     = "Server=${data.aws_rds_cluster.example.endpoint};Database=${data.aws_rds_cluster.example.database_name};Uid=${var.db_user};Pwd=${var.db_pwd};Port=${data.aws_rds_cluster.example.port};"
  jwt_issuer            = var.jwt_issuer
  jwt_aud               = var.jwt_aud
  docker_image          = var.api_docker_image
  cognito_user_pool_id  = data.aws_cognito_user_pools.user_pool.ids[0]
  aws_access_key        = var.api_access_key_id
  aws_secret_access_key = var.api_secret_access_key
  aws_region            = "us-east-1"
}

##############################
# SQS
##############################

# resource "aws_sqs_queue" "bmb-events" {
#   name                       = "bmb-events"
#   delay_seconds              = 0
#   visibility_timeout_seconds = 30
#   receive_wait_time_seconds  = 0
# }


##############################
# CONFIGS/SECRETS
##############################

resource "kubernetes_config_map_v1" "config_map_api" {
  metadata {
    name      = "configmap-api"
    namespace = kubernetes_namespace.orders.metadata.0.name
    labels = {
      "app"       = "api"
      "terraform" = true
    }
  }
  data = {
    "ConnectionStrings__MySql"             = local.connection_string
    "ASPNETCORE_ENVIRONMENT"               = "Development"
    "Serilog__WriteTo__2__Args__serverUrl" = "http://api-internal.fiap-log.svc.cluster.local"
    "Serilog__WriteTo__2__Args__formatter" = "Serilog.Formatting.Json.JsonFormatter, Serilog"
    "Serilog__Enrich__0"                   = "FromLogContext"
    "HybridCache__Expiration"              = "01:00:00"
    "HybridCache__LocalCacheExpiration"    = "01:00:00"
    "HybridCache__Flags"                   = "DisableDistributedCache"
    "JwtOptions__Issuer"                   = local.jwt_issuer
    "JwtOptions__Audience"                 = local.jwt_aud
    "JwtOptions__ExpirationSeconds"        = 3600
    "JwtOptions__UseAccessToken"           = true
    "CognitoSettings__UserPoolId"          = local.cognito_user_pool_id
  }
}

resource "kubernetes_secret" "secret_mercadopago" {
  metadata {
    name      = "secret-api"
    namespace = kubernetes_namespace.orders.metadata.0.name
    labels = {
      app         = "api-pod"
      "terraform" = true
    }
  }
  data = {
    "JwtOptions__SigningKey" = var.jwt_signing_key
    "AWS_SECRET_ACCESS_KEY"  = local.aws_secret_access_key
    "AWS_ACCESS_KEY_ID"      = local.aws_access_key
    "AWS_REGION"             = local.aws_region
  }
  type = "Opaque"
}

####################################
# API
####################################


resource "kubernetes_service" "bmb-api-svc" {
  metadata {
    name      = var.internal_elb_name
    namespace = kubernetes_namespace.orders.metadata.0.name
    annotations = {
      "service.beta.kubernetes.io/aws-load-balancer-type"   = "nlb"
      "service.beta.kubernetes.io/aws-load-balancer-scheme" = "internal"
    }
  }
  spec {
    port {
      port        = 80
      target_port = 8080
      node_port   = 30001
      protocol    = "TCP"
    }
    type = "LoadBalancer"
    selector = {
      app : "api"
    }
  }
}

resource "kubernetes_deployment" "deployment_api" {
  depends_on = [kubernetes_secret.secret_mercadopago, kubernetes_config_map_v1.config_map_api]
  metadata {
    name      = "deployment-api"
    namespace = kubernetes_namespace.orders.metadata.0.name
    labels = {
      app         = "api"
      "terraform" = true
    }
  }
  spec {
    replicas = 1
    selector {
      match_labels = {
        app = "api"
      }
    }
    template {
      metadata {
        name = "pod-api"
        labels = {
          app         = "api"
          "terraform" = true
        }
      }
      spec {
        automount_service_account_token = false
        container {
          name  = "api-container"
          image = local.docker_image
          port {
            name           = "liveness-port"
            container_port = 8080
          }
          port {
            container_port = 80
          }

          image_pull_policy = "IfNotPresent"
          liveness_probe {
            http_get {
              path = "/healthz"
              port = "liveness-port"
            }
            period_seconds        = 10
            failure_threshold     = 3
            initial_delay_seconds = 20
          }
          readiness_probe {
            http_get {
              path = "/healthz"
              port = "liveness-port"
            }
            period_seconds        = 10
            failure_threshold     = 3
            initial_delay_seconds = 10
          }

          resources {
            requests = {
              cpu    = "100m"
              memory = "120Mi"
            }
            limits = {
              cpu    = "150m"
              memory = "200Mi"
            }
          }
          env_from {
            config_map_ref {
              name = "configmap-api"
            }
          }
          env_from {
            secret_ref {
              name = "secret-api"
            }
          }
        }
      }
    }
  }
}

resource "kubernetes_horizontal_pod_autoscaler_v2" "hpa_api" {
  metadata {
    name      = "hpa-api"
    namespace = kubernetes_namespace.orders.metadata.0.name
  }
  spec {
    max_replicas = 3
    min_replicas = 1
    scale_target_ref {
      api_version = "apps/v1"
      kind        = "Deployment"
      name        = "deployment-api"
    }

    metric {
      type = "Resource"
      resource {
        name = "cpu"
        target {
          average_utilization = 65
          type                = "Utilization"
        }
      }
    }
  }
}
