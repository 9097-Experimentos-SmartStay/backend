@US-07
@Bookings
Feature: Gestión centralizada de reservas

  Como recepcionista o administrador de SmartStay
  Quiero gestionar el estado y ciclo de vida de las reservas de las habitaciones
  Para mantener actualizado el control de ocupación y atención a los huéspedes

  @US-07
  @happy-path
  Scenario: Confirmar una reserva pendiente tras verificar el pago
    Given que existe una reserva en estado "PendingPayment" con identificador 501
    When el sistema procesa la confirmación de la reserva con identificador 501
    Then la reserva debe cambiar su estado a "Confirmed"

  @US-07
  @happy-path
  Scenario: Cancelar una reserva por solicitud del huésped
    Given que existe una reserva confirmada con identificador 502 del huésped con identificador 20
    When el huésped solicita cancelar la reserva con identificador 502
    Then la reserva debe cambiar su estado a "Cancelled"

  @US-07
  @happy-path
  Scenario: Reprogramar las fechas de una reserva existente
    Given que existe una reserva activa con identificador 503 para la habitación 101
    When el staff solicita reprogramar la reserva 503 a las fechas "2026-12-10" hasta "2026-12-15"
    Then las fechas de estadía de la reserva deben actualizarse correctamente
