@US-51
@Bookings
Feature: Reserva de habitación por huésped

  Como huésped del hotel SmartStay
  Quiero reservar una habitación disponible para mis fechas de estadía
  Para asegurar mi alojamiento de forma confiable

  @US-51
  @happy-path
  Scenario: Registrar una reserva para una habitación disponible
    Given que existe un hotel registrado y operativo
    And existe una habitación "101" de tipo "Double" disponible con precio por noche de 150.00 soles
    And existe un huésped registrado con correo "juan.perez@smartstay.pe" y nombre "Juan Perez"
    When el huésped solicita registrar una reserva desde "2026-11-01" hasta "2026-11-05"
    Then la reserva debe crearse exitosamente
    And el estado inicial de la reserva debe ser "Pending"
    And la reserva debe contener un código de reserva generado
    And la reserva debe estar asociada a la habitación "101"

  @US-51
  @negative
  Scenario: Rechazar una reserva que se superpone con otra reserva existente
    Given que existe un hotel registrado y operativo
    And existe una habitación "102" de tipo "Single" disponible con precio por noche de 100.00 soles
    And existe una reserva activa para la habitación "102" del "2026-11-10" al "2026-11-15"
    When otro huésped intenta registrar una reserva para la habitación "102" del "2026-11-12" al "2026-11-14"
    Then la solicitud de reserva debe ser rechazada indicando que la habitación no está disponible

  @US-51
  @validation
  Scenario: Rechazar una reserva cuya fecha de salida no sea posterior a la fecha de entrada
    Given que existe un hotel registrado y operativo
    And existe una habitación "103" de tipo "Suite" disponible con precio por noche de 250.00 soles
    And existe un huésped registrado con correo "maria.lopez@smartstay.pe" y nombre "Maria Lopez"
    When el huésped solicita registrar una reserva desde "2026-11-10" hasta "2026-11-08"
    Then la solicitud de reserva debe ser rechazada por rango de fechas inválido

  @US-51
  @boundary
  Scenario: Permitir una reserva que comienza el mismo día en que termina la reserva anterior
    Given que existe un hotel registrado y operativo
    And existe una habitación "104" de tipo "Double" disponible con precio por noche de 180.00 soles
    And existe una reserva activa para la habitación "104" del "2026-11-01" al "2026-11-05"
    When un nuevo huésped solicita registrar una reserva para la habitación "104" del "2026-11-05" al "2026-11-10"
    Then la reserva debe crearse exitosamente
    And el estado inicial de la reserva debe ser "Pending"
