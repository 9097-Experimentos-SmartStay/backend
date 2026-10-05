@US-04
@IAM
Feature: Recuperación de contraseña

  Como usuario registrado de SmartStay
  Quiero solicitar el restablecimiento de mi contraseña mediante un enlace seguro
  Para recuperar el acceso a mi cuenta en caso de olvido

  @US-04
  @happy-path
  Scenario: Solicitar restablecimiento de contraseña para una cuenta existente
    Given que existe un usuario registrado y verificado con correo "recuperacion@smartstay.pe"
    When el usuario solicita la recuperación de su contraseña para el correo "recuperacion@smartstay.pe"
    Then el sistema debe emitir un token seguro de recuperación y enviar el enlace al correo del usuario

  @US-04
  @happy-path
  Scenario: Restablecer exitosamente la contraseña usando el token de recuperación
    Given que el usuario con correo "olvido@smartstay.pe" tiene un token válido de recuperación de contraseña
    When el usuario envía el token de recuperación con la nueva contraseña "NuevaClaveSegura.2026!"
    Then la contraseña del usuario debe actualizarse correctamente
    And el usuario puede iniciar sesión exitosamente con la nueva contraseña "NuevaClaveSegura.2026!"

  @US-04
  @negative
  Scenario: Rechazar el restablecimiento cuando el token es inválido o inexistente
    Given que se tiene un token de recuperación inválido "token_invalido_12345"
    When se intenta restablecer la contraseña con el token inválido y la nueva contraseña "NuevaClaveSegura.2026!"
    Then la solicitud de restablecimiento debe ser rechazada por token inválido
