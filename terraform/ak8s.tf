resource azurerm_kubernetes_cluster main {
  name                = "aks-${var.application_name}-${var.environment_name}-${var.location_short}-${var.resource_version}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  dns_prefix          = "aks-${var.application_name}-${var.environment_name}-${var.location_short}-${var.resource_version}"

  default_node_pool {
    name       = "default"
    node_count = 1
    vm_size    = "Standard_B2s"
  }

  identity {
    type = "SystemAssigned"
  }
}

resource azurerm_role_assignment aks_acr_pull {
  principal_id         = azurerm_kubernetes_cluster.main.kubelet_identity[0].object_id
  role_definition_name = "AcrPull"
  scope                = azurerm_container_registry.main.id
  skip_service_principal_aad_check = true
}