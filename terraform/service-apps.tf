resource azurerm_service_plan main {
  name                = "asp-${var.application_name}-${var.environment_name}-${var.location_short}-${var.resource_version}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  os_type             = "Linux"
  sku_name            = var.sku_name
}

resource azurerm_linux_web_app backend {

  depends_on = [ azurerm_linux_web_app.bank_backend, azurerm_linux_web_app.psp_frontend, azurerm_application_insights.appi_sepwebshop_backend ]

  name                = "lwa-${var.application_name}-be-${var.environment_name}-${var.location_short}-${var.resource_version}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  service_plan_id     = azurerm_service_plan.main.id

  site_config {
    ip_restriction_default_action = "Allow"
    minimum_tls_version           = 1.2
    always_on                     = true

    application_stack {
      dotnet_version = "8.0"
    }
}

  identity {
    type = "SystemAssigned"  
  }
  connection_string {
    name  = "DefaultConnection"
    type  = "SQLAzure"
    value = "Data Source=sql-${var.application_name}-${var.environment_name}-${var.location_short}-${var.resource_version}.database.windows.net,1433;Initial Catalog=sqldb-${var.application_name}-webshopdb-${var.environment_name}-${var.location_short}-${var.resource_version};Persist Security Info=False;User ID=${data.azurerm_key_vault_secret.admin_login.value};Password=${data.azurerm_key_vault_secret.admin_password.value};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
  app_settings = {
    "PSP__FrontendBaseUrl" = "https://${azurerm_linux_web_app.psp_frontend.default_hostname}"
    
    "PSP__PaymentInitEndpoint" = "https://sepapp.xyz/d/Payments/init"

    "SendGrid__ApiKey" = "${data.azurerm_key_vault_secret.sendgrid_api_key.value}"

    "ConfirmEmailBaseUrl" = "https://lwa-sepapp-be-dev-eun-001.azurewebsites.net/api/Users/confirm-email"

    "APPINSIGHTS_INSTRUMENTATIONKEY"           = azurerm_application_insights.appi_sepwebshop_backend.instrumentation_key,
    "APPLICATIONINSIGHTS_CONNECTION_STRING"    = azurerm_application_insights.appi_sepwebshop_backend.connection_string,
    "ApplicationInsightsAgent_EXTENSION_VERSION" = "~3"
  }
}

resource azurerm_linux_web_app frontend {

  depends_on = [ azurerm_application_insights.appi_sepwebshop_frontend ]

  name                = "lwa-${var.application_name}-fe-${var.environment_name}-${var.location_short}-${var.resource_version}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  service_plan_id     = azurerm_service_plan.main.id

  site_config {
    ip_restriction_default_action = "Allow"
    minimum_tls_version           = 1.2
    always_on                     = true
    app_command_line              = "pm2 serve /home/site/wwwroot --no-daemon --spa"
    application_stack {
      node_version = "22-lts"
    }
  }
  app_settings = {
      "APPINSIGHTS_INSTRUMENTATIONKEY"           = azurerm_application_insights.appi_sepwebshop_frontend.instrumentation_key,
      "APPLICATIONINSIGHTS_CONNECTION_STRING"    = azurerm_application_insights.appi_sepwebshop_frontend.connection_string,
      "ApplicationInsightsAgent_EXTENSION_VERSION" = "~3"
    }
}

resource azurerm_linux_web_app psp_frontend {

  depends_on = [ azurerm_application_insights.appi_psp_frontend ]

  name                = "lwa-${var.application_name}-fe-psp-${var.environment_name}-${var.location_short}-${var.resource_version}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  service_plan_id     = azurerm_service_plan.main.id

  site_config {
    ip_restriction_default_action = "Allow"
    minimum_tls_version           = 1.2
    always_on                     = true
    app_command_line              = "pm2 serve /home/site/wwwroot --no-daemon --spa"
    application_stack {
      node_version = "22-lts"
    }
  }
  app_settings = {
      "APPINSIGHTS_INSTRUMENTATIONKEY"           = azurerm_application_insights.appi_psp_frontend.instrumentation_key,
      "APPLICATIONINSIGHTS_CONNECTION_STRING"    = azurerm_application_insights.appi_psp_frontend.connection_string,
      "ApplicationInsightsAgent_EXTENSION_VERSION" = "~3"
    }
}

resource azurerm_linux_web_app bank_frontend {

  depends_on = [ azurerm_linux_web_app.frontend, azurerm_application_insights.appi_bank_frontend ]

  name                = "lwa-${var.application_name}-fe-bank-${var.environment_name}-${var.location_short}-${var.resource_version}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  service_plan_id     = azurerm_service_plan.main.id

  site_config {
    ip_restriction_default_action = "Allow"
    minimum_tls_version           = 1.2
    always_on                     = true
    app_command_line              = "pm2 serve /home/site/wwwroot --no-daemon --spa"
    application_stack {
      node_version = "22-lts"
    }
  }
  app_settings = {
      "APPINSIGHTS_INSTRUMENTATIONKEY"           = azurerm_application_insights.appi_bank_frontend.instrumentation_key,
      "APPLICATIONINSIGHTS_CONNECTION_STRING"    = azurerm_application_insights.appi_bank_frontend.connection_string,
      "ApplicationInsightsAgent_EXTENSION_VERSION" = "~3"
    }
}

resource azurerm_linux_web_app bank_backend {

  depends_on = [ azurerm_linux_web_app.bank_frontend, azurerm_linux_web_app.frontend, azurerm_application_insights.appi_bank_backend ]

  name                = "lwa-${var.application_name}-be-bank-${var.environment_name}-${var.location_short}-${var.resource_version}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  service_plan_id     = azurerm_service_plan.main.id

  site_config {
    ip_restriction_default_action = "Allow"
    minimum_tls_version           = 1.2
    always_on                     = true

    application_stack {
      dotnet_version = "8.0"
    }
}

  identity {
    type = "SystemAssigned"  
  }
  connection_string {
    name  = "DefaultConnection"
    type  = "SQLAzure"
    value = "Data Source=sql-${var.application_name}-${var.environment_name}-${var.location_short}-${var.resource_version}.database.windows.net,1433;Initial Catalog=sqldb-${var.application_name}-bankdb-${var.environment_name}-${var.location_short}-${var.resource_version};Persist Security Info=False;User ID=${data.azurerm_key_vault_secret.admin_login.value};Password=${data.azurerm_key_vault_secret.admin_password.value};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
  app_settings = {
    "BankFrontendUrl" = "https://${azurerm_linux_web_app.bank_frontend.default_hostname}"
    "ApiSettings__PspCardBaseUrl" = "https://sepapp.xyz/"
    "ApiSettings__PspQrBaseUrl" = "https://sepapp.xyz/"
    "ApiSettings__WebShopSuccessUrl" = "https://${azurerm_linux_web_app.frontend.default_hostname}/success"

    "APPINSIGHTS_INSTRUMENTATIONKEY"           = azurerm_application_insights.appi_bank_backend.instrumentation_key,
    "APPLICATIONINSIGHTS_CONNECTION_STRING"    = azurerm_application_insights.appi_bank_backend.connection_string,
    "ApplicationInsightsAgent_EXTENSION_VERSION" = "~3"
  }
}