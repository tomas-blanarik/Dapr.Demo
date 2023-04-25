locals {
  container_apps_apis = {
    "users-api"    = { image_name = "dapr-users-api" },
    "payment-api"  = { image_name = "dapr-payment-api" },
    "ordering-api" = { image_name = "dapr-ordering-api" },
    "audit-api"    = { image_name = "dapr-audit-api" },
    "basket-api"   = { image_name = "basket-api" }
  }
}