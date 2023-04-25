resource "azurerm_log_analytics_workspace" "log" {
  name                = "log-dapr-demo"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
}

resource "azurerm_container_app_environment" "cae" {
  name                       = "cae-dapr-demo"
  location                   = azurerm_resource_group.rg.location
  resource_group_name        = azurerm_resource_group.rg.name
  log_analytics_workspace_id = azurerm_log_analytics_workspace.log.id
}

resource "azurerm_container_app_environment_dapr_component" "pubsub" {
  name                         = "service-bus"
  container_app_environment_id = azurerm_container_app_environment.cae.id
  component_type               = "pubsub.azure.servicebus"
  version                      = "v1"

  metadata {
    name  = "connectionString"
    value = azurerm_servicebus_namespace.sb.default_primary_connection_string
  }

  scopes = [
    "users-api",
    "audit-api",
    "ordering-api",
    "payment-api",
    "basket-api"
  ]
}

resource "azurerm_container_app_environment_dapr_component" "secretstore" {
  name                         = "secretstore"
  container_app_environment_id = azurerm_container_app_environment.cae.id
  component_type               = "secretstores.azure.keyvault"
  version                      = "v1"
  ignore_errors                = false

  scopes = [
    "users-api",
    "audit-api",
    "ordering-api",
    "payment-api",
    "basket-api"
  ]

  metadata {
    name  = "vaultName"
    value = azurerm_key_vault.key_vault.name
  }
}

resource "azurerm_container_app_environment_dapr_component" "statestore" {
  name                         = "statestore"
  container_app_environment_id = azurerm_container_app_environment.cae.id
  component_type               = "state.redis"
  version                      = "v1"
  ignore_errors                = false

  scopes = [
    "basket-api"
  ]

  secret {
    name = "redis-password"
    value = azurerm_redis_cache.redis.primary_access_key
  }

  metadata {
    name  = "redisHost"
    value = azurerm_redis_cache.redis.hostname
  }

  metadata {
    name = "redisPassword"
    secret_name = "redis-password"
  }
}