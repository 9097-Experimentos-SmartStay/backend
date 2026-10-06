@US-01
@IAM
Feature: Registro de usuario con validación

  Como nuevo huésped de SmartStay
  Quiero registrar una cuenta con mis datos personales y una contraseña segura
  Para acceder a los servicios de reserva y autogestión de la plataforma

  @US-01
  @happy-path
  Scenario: Registrar exitosamente un nuevo usuario huésped
    Given que el correo electrónico "nuevo.huesped@smartstay.pe" no se encuentra registrado en el sistema
    When el usuario solicita registrarse con nombre "Carlos", apellido "Benitez", correo "nuevo.huesped@smartstay.pe" y contraseña "PasswordSegura.2026!"
    Then el usuario debe registrarse exitosamente en el sistema
    And el rol asignado al usuario debe ser "Guest"
    And el estado inicial de verificación de correo debe ser falso

  @US-01
  @negative
  Scenario: Rechazar el registro cuando el correo electrónico ya está en uso
    Given que ya existe un usuario registrado con el correo "registrado@smartstay.pe"
    When otro usuario intenta registrarse con el correo "registrado@smartstay.pe" y contraseña "OtraClave.2026!"
    Then la solicitud de registro debe ser rechazada indicando que el correo ya está registrado

  @US-01
  @validation
  Scenario: Rechazar el registro cuando la contraseña no cumple la longitud mínima
    Given que el correo electrónico "invalido@smartstay.pe" no se encuentra registrado en el sistema
    When el usuario solicita registrarse con nombre "Ana", apellido "Torres", correo "invalido@smartstay.pe" y contraseña "123"
    Then la solicitud de registro debe ser rechazada por contraseña no permitida
