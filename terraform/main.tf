resource azurerm_resource_group main {
  name     = "rg-${var.application_name}-${var.environment_name}-${var.location_short}-${var.resource_version}"
  location = "${var.location}"
  tags = {
    environment = "${var.environment_name}"
    application = "${var.application_name}"
    location    = "${var.location_short}"
  }
}