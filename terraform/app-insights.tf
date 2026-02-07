resource azurerm_application_insights appi_sepwebshop_backend {
  name                = "appi-${var.application_name}-be-sepwebshop-${var.environment_name}-${var.location_short}-${var.resource_version}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  workspace_id        = azurerm_log_analytics_workspace.main.id
  application_type    = "web"
}

resource azurerm_application_insights appi_sepwebshop_frontend {
  name                = "appi-${var.application_name}-fe-sepwebshop-${var.environment_name}-${var.location_short}-${var.resource_version}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  workspace_id        = azurerm_log_analytics_workspace.main.id
  application_type    = "Node.JS"
}

resource azurerm_application_insights appi_bank_backend {
  name                = "appi-${var.application_name}-be-bank-${var.environment_name}-${var.location_short}-${var.resource_version}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  workspace_id        = azurerm_log_analytics_workspace.main.id
  application_type    = "web"
}

resource azurerm_application_insights appi_bank_frontend {
  name                = "appi-${var.application_name}-fe-bank-${var.environment_name}-${var.location_short}-${var.resource_version}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  workspace_id        = azurerm_log_analytics_workspace.main.id
  application_type    = "Node.JS"
}

resource azurerm_application_insights appi_psp_frontend {
  name                = "appi-${var.application_name}-fe-psp-${var.environment_name}-${var.location_short}-${var.resource_version}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  workspace_id        = azurerm_log_analytics_workspace.main.id
  application_type    = "Node.JS"
}