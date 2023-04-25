variable "rg_name" {
  description = "Name of the resource group to be created"
  type        = string
  default     = "rg-dapr-demo"
}

variable "location" {
  description = "Region in which to create the resources. Defaults to `West Europe`"
  type        = string
  default     = "westeurope"
}

variable "environment" {
  description = "Current .NET environment (Development, Production)"
  type        = string
  default     = "Production"
}

variable "max_replicas" {
  description = "Maximum number of replicas per container app"
  type        = number
  default     = 1
}

variable "container_registry_host" {
  description = "Container registry host"
  type        = string
}

variable "container_registry_username" {
  description = "Container registry username"
  type        = string
}

variable "container_registry_password" {
  description = "Container registry password"
  type        = string
  sensitive   = true
}

variable "container_registry_namespace" {
  description = "Container registry password"
  type        = string
}

variable "database_user" {
  description = "Database admin username"
  type        = string
}

variable "database_password" {
  description = "Database admin password"
  type        = string
  sensitive   = true
}