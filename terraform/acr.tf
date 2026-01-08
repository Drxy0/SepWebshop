resource azurerm_container_registry main {
  name                = "acr${var.application_name}${var.environment_name}${var.resource_version}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = "Basic"
  admin_enabled       = true
}