resource azurerm_mssql_server main {
  name                         = "sql-${var.application_name}-${var.environment_name}-${var.location_short}-${var.resource_version}"
  resource_group_name          = azurerm_resource_group.main.name
  location                     = azurerm_resource_group.main.location
  version                      = "12.0"
  administrator_login          = data.azurerm_key_vault_secret.admin_login.value
  administrator_login_password = data.azurerm_key_vault_secret.admin_password.value
  minimum_tls_version          = "1.2"

}

resource azurerm_mssql_firewall_rule allow_azure_services {
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

resource azurerm_mssql_database webshopdb {
  name      = "sqldb-${var.application_name}-webshopdb-${var.environment_name}-${var.location_short}-${var.resource_version}"
  server_id = azurerm_mssql_server.main.id
  sku_name  = "Basic"
}

resource azurerm_mssql_database dataservicedb {
  name      = "sqldb-${var.application_name}-dataservicedb-${var.environment_name}-${var.location_short}-${var.resource_version}"
  server_id = azurerm_mssql_server.main.id
  sku_name  = "Basic"
}

resource azurerm_mssql_database cardservicedb {
  name      = "sqldb-${var.application_name}-cardservicedb-${var.environment_name}-${var.location_short}-${var.resource_version}"
  server_id = azurerm_mssql_server.main.id
  sku_name  = "Basic"
}