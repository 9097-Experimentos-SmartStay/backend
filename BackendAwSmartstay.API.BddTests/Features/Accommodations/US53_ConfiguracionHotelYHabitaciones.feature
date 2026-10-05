@US-53
@Accommodations
Feature: Configuración de hotel y habitaciones

  Como administrador del hotel
  Quiero registrar y configurar habitaciones con su información y tarifas
  Para ponerlas a disposición de los huéspedes del sistema

  @US-53
  @happy-path
  Scenario: Registrar una habitación con precio y datos válidos
    Given que existe un hotel con identificador 1 y un tipo de habitación con identificador 1
    When el administrador registra una nueva habitación con número "201", precio de 120.00 soles y descripción "Habitación Estándar con vista"
    Then la habitación debe crearse exitosamente
    And la habitación debe tener el número "201"
    And la habitación debe tener el precio de 120.00 soles
    And el estado inicial de la habitación debe ser "Available"

  @US-53
  @validation
  Scenario: Rechazar el registro de una habitación con precio menor o igual a cero
    Given que existe un hotel con identificador 1 y un tipo de habitación con identificador 1
    When el administrador intenta registrar una habitación con número "202" y precio de 0.00 soles
    Then la solicitud de registro debe ser rechazada indicando precio fuera de rango

  @US-53
  @negative
  Scenario: Rechazar el registro de una habitación con número duplicado en el mismo hotel
    Given que existe un hotel con identificador 1 y un tipo de habitación con identificador 1
    And ya existe una habitación registrada con número "203" en el hotel 1
    When el administrador intenta registrar otra habitación con el mismo número "203" en el hotel 1
    Then la solicitud de registro debe ser rechazada por número de habitación duplicado
