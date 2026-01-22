resource azurerm_service_plan main {
  name                = "asp-${var.application_name}-${var.environment_name}-${var.location_short}-${var.resource_version}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  os_type             = "Linux"
  sku_name            = var.sku_name
}

resource azurerm_linux_web_app backend {
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
    
    "PSP__MerchantId"      = "MERCHANT_001"
    // "PSP__MerchantPassword" = "" // TODO
    "Jwt__Issuer"          = "SepWebshop"
    "Jwt__Audience"        = "SepWebshopClients"
    "AllowedHosts"         = "*"
  }
}

resource azurerm_linux_web_app frontend {
  name                = "lwa-${var.application_name}-fe-${var.environment_name}-${var.location_short}-${var.resource_version}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  service_plan_id     = azurerm_service_plan.main.id

  site_config {
    ip_restriction_default_action = "Allow"
    minimum_tls_version           = 1.2
    always_on                     = true
    app_command_line              = "npx serve -s ."
    application_stack {
      node_version = "22-lts"
    }
  }
}

resource azurerm_linux_web_app psp_frontend {
  name                = "lwa-${var.application_name}-fe-psp-${var.environment_name}-${var.location_short}-${var.resource_version}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  service_plan_id     = azurerm_service_plan.main.id

  site_config {
    ip_restriction_default_action = "Allow"
    minimum_tls_version           = 1.2
    always_on                     = true
    app_command_line              = "npx serve -s ."
    application_stack {
      node_version = "22-lts"
    }
  }
}

resource azurerm_linux_web_app card_service {
  name                = "lwa-${var.application_name}-card-service-${var.environment_name}-${var.location_short}-${var.resource_version}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  service_plan_id     = azurerm_service_plan.main.id

  site_config {
    ip_restriction_default_action = "Allow"
    minimum_tls_version           = 1.2
    always_on                     = true

    application_stack {
      java_server         = "JAVA"
      java_server_version = "17"
      java_version        = "17"
    }
  }

  identity {
    type = "SystemAssigned"
  }

  app_settings = {
    "SPRING_DATASOURCE_URL"              = "jdbc:sqlserver://sql-${var.application_name}-${var.environment_name}-${var.location_short}-${var.resource_version}.database.windows.net:1433;databaseName=sqldb-${var.application_name}-cardservicedb-${var.environment_name}-${var.location_short}-${var.resource_version};encrypt=true;trustServerCertificate=false"
    "SPRING_DATASOURCE_USERNAME"         = data.azurerm_key_vault_secret.admin_login.value
    "SPRING_DATASOURCE_PASSWORD"         = data.azurerm_key_vault_secret.admin_password.value
    "SPRING_DATASOURCE_DRIVER_CLASS_NAME" = "com.microsoft.sqlserver.jdbc.SQLServerDriver"
    "ACQUIRER_MERCHANT_ID"               = "MERCHANT_001"
    "SERVER_PORT"                        = "8080"
  }
}