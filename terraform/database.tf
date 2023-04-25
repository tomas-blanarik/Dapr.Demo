resource "azurerm_mysql_flexible_server" "mysql" {
  name                   = "mysql-flexible-dapr-demo"
  resource_group_name    = azurerm_resource_group.rg.name
  location               = azurerm_resource_group.rg.location
  administrator_login    = var.database_user
  administrator_password = var.database_password
  sku_name               = "GP_Standard_D2ds_v4"
}