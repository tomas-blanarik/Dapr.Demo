# Container Apps for APIs
resource "azurerm_container_app" "ca_api" {
  for_each                     = local.container_apps_apis
  name                         = "ca-dapr-demo-${each.key}"
  container_app_environment_id = azurerm_container_app_environment.cae.id
  resource_group_name          = azurerm_resource_group.rg.name
  revision_mode                = "Single"

  dapr {
    app_id   = each.key
    app_port = 80
  }

  ingress {
    target_port      = 80
    external_enabled = false
    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }

  secret {
    name  = "registry-password"
    value = var.container_registry_password
  }

  registry {
    server               = var.container_registry_host
    username             = var.container_registry_username
    password_secret_name = "registry-password"
  }

  template {
    min_replicas = 0
    max_replicas = var.max_replicas

    container {
      name   = each.key
      image  = "${var.container_registry_host}/${var.container_registry_namespace}/${each.value.image_name}:latest"
      cpu    = 0.5
      memory = "1Gi"

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = var.environment
      }
    }
  }

  lifecycle {
    ignore_changes = [
      ingress,
      secret,
      template[0].container[0].image
    ]
  }
}