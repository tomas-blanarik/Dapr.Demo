resource "azurerm_key_vault" "key_vault" {
  name                        = "kv-dapr-demo"
  location                    = azurerm_resource_group.rg.location
  resource_group_name         = azurerm_resource_group.rg.name
  enabled_for_disk_encryption = true
  tenant_id                   = data.azurerm_client_config.current.tenant_id
  soft_delete_retention_days  = 7
  purge_protection_enabled    = false

  sku_name = "standard"

  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = data.azurerm_client_config.current.object_id

    key_permissions = [
      "Get",
    ]

    secret_permissions = [
      "Get",
    ]

    storage_permissions = [
      "Get",
    ]
  }
}

resource "azurerm_key_vault_secret" "cs_audi_api" {
  name         = "ConnectionStrings--AuditDbConnection"
  value        = "Server=${azurerm_mysql_flexible_server.mysql.fqdn};Database=audit-db;Uid=${var.database_user};Pwd=${var.database_password};"
  key_vault_id = azurerm_key_vault.key_vault.id
}

resource "azurerm_key_vault_secret" "cs_ordering_api" {
  name         = "ConnectionStrings--OrdersDbConnection"
  value        = "Server=${azurerm_mysql_flexible_server.mysql.fqdn};Database=ordering-db;Uid=${var.database_user};Pwd=${var.database_password};"
  key_vault_id = azurerm_key_vault.key_vault.id
}

resource "azurerm_key_vault_secret" "cs_users_api" {
  name         = "ConnectionStrings--UsersDbConnection"
  value        = "Server=${azurerm_mysql_flexible_server.mysql.fqdn};Database=users-db;Uid=${var.database_user};Pwd=${var.database_password};"
  key_vault_id = azurerm_key_vault.key_vault.id
}

resource "azurerm_key_vault_secret" "cs_payment_api" {
  name         = "ConnectionStrings--PaymentsDbConnection"
  value        = "Server=${azurerm_mysql_flexible_server.mysql.fqdn};Database=payment-db;Uid=${var.database_user};Pwd=${var.database_password};"
  key_vault_id = azurerm_key_vault.key_vault.id
}