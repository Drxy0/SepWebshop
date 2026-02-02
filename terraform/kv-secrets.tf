data azurerm_key_vault main {
  name                = "kv-sepapp-mng"
  resource_group_name = "rg-sepapp-mng-eun"
}

data azurerm_key_vault_secret admin_login {
  name         = "sql-admin"
  key_vault_id = data.azurerm_key_vault.main.id
}

data azurerm_key_vault_secret admin_password {
  name         = "sql-password"
  key_vault_id = data.azurerm_key_vault.main.id
}

data azurerm_key_vault_secret sendgrid_api_key {
  name         = "sendgrid-api-key"
  key_vault_id = data.azurerm_key_vault.main.id
}