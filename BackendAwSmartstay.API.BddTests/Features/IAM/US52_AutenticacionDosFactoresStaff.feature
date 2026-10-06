@US-52
@IAM
Feature: Autenticación de dos factores para staff

  Como miembro del personal del hotel
  Quiero contar con autenticación de doble factor (2FA/MFA) basada en TOTP
  Para proteger el acceso a las funciones administrativas del sistema

  @US-52
  @happy-path
  Scenario: Iniciar y confirmar exitosamente el enrolamiento en MFA
    Given que existe un usuario del staff con correo "staff.mfa@smartstay.pe" y contraseña "ClaveStaffSegura.2026!"
    When el usuario solicita iniciar el enrolamiento en autenticación de dos factores
    And confirma el enrolamiento enviando el código TOTP generado por su aplicación
    Then la autenticación de dos factores debe quedar habilitada para el usuario
    And se deben generar códigos de recuperación de un solo uso

  @US-52
  @happy-path
  Scenario: Iniciar sesión exitosamente requiriendo y verificando el segundo factor
    Given que el usuario del staff "staff.activo@smartstay.pe" tiene MFA habilitado con su clave secreta
    When el usuario completa el primer factor de autenticación con su contraseña "ClaveStaffSegura.2026!"
    And envía el código TOTP correcto para el segundo factor
    Then la sesión debe ser otorgada con un token de acceso JWT válido

  @US-52
  @negative
  Scenario: Rechazar el segundo factor cuando el código TOTP es inválido
    Given que el usuario del staff "staff.bloqueo@smartstay.pe" tiene MFA habilitado con su clave secreta
    When el usuario intenta verificar el segundo factor con un código inválido "000000"
    Then la verificación de dos factores debe ser rechazada por código inválido
