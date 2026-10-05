@US-03
@Profiles
Feature: Gestión de perfiles y roles

  Como administrador del sistema SmartStay
  Quiero gestionar los perfiles de los colaboradores y sus asignaciones a hoteles
  Para controlar las responsabilidades y permisos operativos del personal

  @US-03
  @happy-path
  Scenario: Crear exitosamente un perfil de colaborador para un usuario
    Given que existe un usuario del sistema con identificador 100
    When el administrador crea el perfil de personal con cargo "Recepcionista", turno "Mañana" y correo "recepcion@smartstay.pe"
    Then el perfil de personal debe registrarse correctamente con un código de empleado generado
    And el perfil debe estar vinculado al usuario 100

  @US-03
  @happy-path
  Scenario: Asignar un colaborador a un hotel con rol operativo
    Given que existe un hotel registrado con identificador 1
    And existe un perfil de colaborador registrado en el sistema
    When el administrador asigna al colaborador al hotel 1 con el rol "Reception"
    Then la asignación al hotel debe registrarse exitosamente en el perfil del colaborador

  @US-03
  @negative
  Scenario: Rechazar la asignación cuando el hotel de destino no existe
    Given que no existe ningún hotel con identificador 999
    And existe un perfil de colaborador registrado en el sistema
    When el administrador intenta asignar al colaborador al hotel 999 con el rol "Reception"
    Then la solicitud de asignación debe ser rechazada indicando que el hotel no existe
