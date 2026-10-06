@US-02
@IAM
Feature: Inicio de sesión seguro

  Como usuario registrado de SmartStay
  Quiero autenticarme con mis credenciales de forma segura
  Para acceder a las funciones del sistema según mis permisos

  @US-02
  @happy-path
  Scenario: Iniciar sesión exitosamente con credenciales válidas
    Given que existe un usuario registrado con correo "huesped.valido@smartstay.pe" y contraseña "ClaveSegura.2026!"
    When el usuario solicita iniciar sesión con correo "huesped.valido@smartstay.pe" y contraseña "ClaveSegura.2026!"
    Then el inicio de sesión debe ser exitoso
    And debe recibir un token de acceso JWT válido

  @US-02
  @negative
  Scenario: Rechazar inicio de sesión con contraseña incorrecta
    Given que existe un usuario registrado con correo "usuario.pass@smartstay.pe" y contraseña "ClaveSegura.2026!"
    When el usuario solicita iniciar sesión con correo "usuario.pass@smartstay.pe" y contraseña "ClaveErronea.2026!"
    Then la autenticación debe fallar con error de credenciales inválidas

  @US-02
  @security
  Scenario: Bloqueo temporal de la cuenta tras múltiples intentos fallidos consecutivos
    Given que existe un usuario registrado con correo "bloqueo@smartstay.pe" y contraseña "ClaveSegura.2026!"
    When el usuario falla el inicio de sesión 5 veces consecutivas con una contraseña errónea
    Then la cuenta del usuario debe quedar bloqueada temporalmente
    And un intento posterior debe ser rechazado por cuenta bloqueada
