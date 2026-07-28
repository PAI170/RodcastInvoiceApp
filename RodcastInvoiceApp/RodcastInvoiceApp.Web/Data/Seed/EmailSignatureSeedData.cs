namespace RodcastInvoiceApp.Web.Data.Seed
{
    // Firmas HTML fijas por usuario. Todavia no hay pantalla para cargarlas a mano,
    // asi que se siembran una sola vez al arrancar (ver Program.cs) si el usuario
    // todavia no tiene EmailSignatureHtml seteado.
    public static class EmailSignatureSeedData
    {
        public const string DavidRodriguezEmail = "info@rodcastsolutions.com";

        public const string DavidRodriguezHtml = """
            <table cellpadding="0" cellspacing="0" border="0" style="font-family: Verdana, Geneva, sans-serif; font-size: 14px; color: #000000;">
              <tr>
                <td style="vertical-align: top; padding-right: 18px; font-family: Verdana, Geneva, sans-serif;">
                  <table cellpadding="0" cellspacing="0" border="0" style="font-family: Verdana, Geneva, sans-serif;">
                    <tr>
                      <td style="text-align: center; padding-bottom: 10px; font-family: Verdana, Geneva, sans-serif;">
                        <img src="https://www.conofamily.org/wp-content/uploads/2026/04/rodcast_solutions_transparent-1-1.png" width="120" alt="Rodcast Solutions" style="display: block; max-width: 120px; border: 0;">
                      </td>
                    </tr>
                    <tr>
                      <td style="font-family: Verdana, Geneva, sans-serif;">
                        <table cellpadding="0" cellspacing="0" border="0" style="margin: 0 auto;">
                          <tr>
                            <td width="30" height="30" align="center" valign="middle" style="background-color: #28A5D2; border-radius: 50%; font-family: Verdana, Geneva, sans-serif;">
                              <a href="https://www.facebook.com/rodcastsolutions" style="text-decoration: none;"><img src="https://cdn2.hubspot.net/hubfs/53/tools/email-signature-generator/icons/facebook-icon-dark-2x.png" width="17" height="17" alt="Facebook" style="display: block; border: 0;"></a>
                            </td>
                            <td width="6" style="font-size: 1px; line-height: 1px;">&nbsp;</td>
                            <td width="30" height="30" align="center" valign="middle" style="background-color: #28A5D2; border-radius: 50%; font-family: Verdana, Geneva, sans-serif;">
                              <a href="https://www.instagram.com/rodcastsolutionscr/" style="text-decoration: none;"><img src="https://cdn2.hubspot.net/hubfs/53/tools/email-signature-generator/icons/instagram-icon-dark-2x.png" width="17" height="17" alt="Instagram" style="display: block; border: 0;"></a>
                            </td>
                            <td width="6" style="font-size: 1px; line-height: 1px;">&nbsp;</td>
                            <td width="30" height="30" align="center" valign="middle" style="background-color: #28A5D2; border-radius: 50%; font-family: Verdana, Geneva, sans-serif;">
                              <a href="https://wa.me/50672673428" style="text-decoration: none;"><img src="https://cdn2.hubspot.net/hubfs/53/tools/email-signature-generator/icons/whatsapp-icon-dark-2x.png" width="17" height="17" alt="WhatsApp" style="display: block; border: 0;"></a>
                            </td>
                          </tr>
                        </table>
                      </td>
                    </tr>
                  </table>
                </td>
                <td style="vertical-align: middle; border-left: 1px solid #1D5EA0; padding-left: 18px; font-family: Verdana, Geneva, sans-serif;">
                  <div style="font-family: Verdana, Geneva, sans-serif; font-size: 18px; font-weight: 700; color: #000000; line-height: 24px;">David Rodriguez Perez</div>
                  <div style="font-family: Verdana, Geneva, sans-serif; font-size: 14px; color: #000000; line-height: 20px; padding-top: 2px;">Founder & CEO</div>
                  <div style="font-family: Verdana, Geneva, sans-serif; font-size: 14px; font-weight: 600; color: #000000; line-height: 20px; padding-bottom: 10px;">Executive&nbsp;|&nbsp;Rodcast Solutions</div>

                  <table cellpadding="0" cellspacing="0" border="0" style="font-family: Verdana, Geneva, sans-serif; font-size: 14px;">
                    <tr>
                      <td width="20" style="text-align: center; vertical-align: middle; font-family: Verdana, Geneva, sans-serif;">
                        <img src="https://www.conofamily.org/wp-content/uploads/2026/07/call_24dp_1D5EA0_FILL0_wght400_GRAD0_opsz24.png" width="16" height="16" alt="Tel" style="display: block; margin: 0 auto; border: 0;">
                      </td>
                      <td style="padding: 2px 0 2px 8px; vertical-align: middle; font-family: Verdana, Geneva, sans-serif; font-size: 14px; color: #000000;">
                        <a href="tel:+50672673428" style="text-decoration: none; color: #000000; font-family: Verdana, Geneva, sans-serif; font-size: 14px;"><span style="color: #000000 !important; text-decoration: none !important; font-family: Verdana, Geneva, sans-serif; font-size: 14px;">+506 7267-3428</span></a>
                      </td>
                    </tr>
                    <tr>
                      <td width="20" style="text-align: center; vertical-align: middle; font-family: Verdana, Geneva, sans-serif;">
                        <img src="https://www.conofamily.org/wp-content/uploads/2026/07/mail_24dp_1D5EA0_FILL0_wght400_GRAD0_opsz24.png" width="16" height="16" alt="Email" style="display: block; margin: 0 auto; border: 0;">
                      </td>
                      <td style="padding: 2px 0 2px 8px; vertical-align: middle; font-family: Verdana, Geneva, sans-serif; font-size: 14px; color: #000000;">
                        <a href="mailto:info@rodcastsolutions.com" style="text-decoration: none; color: #000000; font-family: Verdana, Geneva, sans-serif; font-size: 14px;"><span style="color: #000000 !important; text-decoration: none !important; font-family: Verdana, Geneva, sans-serif; font-size: 14px;">info@rodcastsolutions.com</span></a>
                      </td>
                    </tr>
                    <tr>
                      <td width="20" style="text-align: center; vertical-align: middle; font-family: Verdana, Geneva, sans-serif;">
                        <img src="https://www.conofamily.org/wp-content/uploads/2026/07/public_24dp_1D5EA0_FILL0_wght400_GRAD0_opsz24.png" width="16" height="16" alt="Web" style="display: block; margin: 0 auto; border: 0;">
                      </td>
                      <td style="padding: 2px 0 2px 8px; vertical-align: middle; font-family: Verdana, Geneva, sans-serif; font-size: 14px; color: #000000;">
                        <a href="https://www.rodcastsolutions.com" style="text-decoration: none; color: #000000; font-family: Verdana, Geneva, sans-serif; font-size: 14px;"><span style="color: #000000 !important; text-decoration: none !important; font-family: Verdana, Geneva, sans-serif; font-size: 14px;">www.rodcastsolutions.com</span></a>
                      </td>
                    </tr>
                  </table>
                </td>
              </tr>
            </table>
            """;

        public const string DanielaCastroEmail = "daniela.castro@rodcastsolutions.com";

        public const string DanielaCastroHtml = """
            <table cellpadding="0" cellspacing="0" border="0" style="font-family: Verdana, Geneva, sans-serif; font-size: 14px; color: #000000;">
              <tr>
                <td style="vertical-align: top; padding-right: 18px; font-family: Verdana, Geneva, sans-serif;">
                  <table cellpadding="0" cellspacing="0" border="0" style="font-family: Verdana, Geneva, sans-serif;">
                    <tr>
                      <td style="text-align: center; padding-bottom: 10px; font-family: Verdana, Geneva, sans-serif;">
                        <img src="https://www.conofamily.org/wp-content/uploads/2026/04/rodcast_solutions_transparent-1-1.png" width="120" alt="Rodcast Solutions" style="display: block; max-width: 120px; border: 0;">
                      </td>
                    </tr>
                    <tr>
                      <td style="font-family: Verdana, Geneva, sans-serif;">
                        <table cellpadding="0" cellspacing="0" border="0" style="margin: 0 auto;">
                          <tr>
                            <td width="30" height="30" align="center" valign="middle" style="background-color: #28A5D2; border-radius: 50%; font-family: Verdana, Geneva, sans-serif;">
                              <a href="https://www.facebook.com/rodcastsolutions" style="text-decoration: none;"><img src="https://cdn2.hubspot.net/hubfs/53/tools/email-signature-generator/icons/facebook-icon-dark-2x.png" width="17" height="17" alt="Facebook" style="display: block; border: 0;"></a>
                            </td>
                            <td width="6" style="font-size: 1px; line-height: 1px;">&nbsp;</td>
                            <td width="30" height="30" align="center" valign="middle" style="background-color: #28A5D2; border-radius: 50%; font-family: Verdana, Geneva, sans-serif;">
                              <a href="https://www.instagram.com/rodcastsolutionscr/" style="text-decoration: none;"><img src="https://cdn2.hubspot.net/hubfs/53/tools/email-signature-generator/icons/instagram-icon-dark-2x.png" width="17" height="17" alt="Instagram" style="display: block; border: 0;"></a>
                            </td>
                            <td width="6" style="font-size: 1px; line-height: 1px;">&nbsp;</td>
                            <td width="30" height="30" align="center" valign="middle" style="background-color: #28A5D2; border-radius: 50%; font-family: Verdana, Geneva, sans-serif;">
                              <a href="https://wa.me/50672860858" style="text-decoration: none;"><img src="https://cdn2.hubspot.net/hubfs/53/tools/email-signature-generator/icons/whatsapp-icon-dark-2x.png" width="17" height="17" alt="WhatsApp" style="display: block; border: 0;"></a>
                            </td>
                          </tr>
                        </table>
                      </td>
                    </tr>
                  </table>
                </td>
                <td style="vertical-align: middle; border-left: 1px solid #1D5EA0; padding-left: 18px; font-family: Verdana, Geneva, sans-serif;">
                  <div style="font-family: Verdana, Geneva, sans-serif; font-size: 18px; font-weight: 700; color: #000000; line-height: 24px;">Daniela Castro Sequeira</div>
                  <div style="font-family: Verdana, Geneva, sans-serif; font-size: 14px; color: #000000; line-height: 20px; padding-top: 2px;">Service Delivery Manager</div>
                  <div style="font-family: Verdana, Geneva, sans-serif; font-size: 14px; font-weight: 600; color: #000000; line-height: 20px; padding-bottom: 10px;">Administration&nbsp;|&nbsp;Rodcast Solutions</div>

                  <table cellpadding="0" cellspacing="0" border="0" style="font-family: Verdana, Geneva, sans-serif; font-size: 14px;">
                    <tr>
                      <td width="20" style="text-align: center; vertical-align: middle; font-family: Verdana, Geneva, sans-serif;">
                        <img src="https://www.conofamily.org/wp-content/uploads/2026/07/call_24dp_1D5EA0_FILL0_wght400_GRAD0_opsz24.png" width="16" height="16" alt="Tel" style="display: block; margin: 0 auto; border: 0;">
                      </td>
                      <td style="padding: 2px 0 2px 8px; vertical-align: middle; font-family: Verdana, Geneva, sans-serif; font-size: 14px; color: #000000;">
                        <a href="tel:+50672860858" style="text-decoration: none; color: #000000; font-family: Verdana, Geneva, sans-serif; font-size: 14px;"><span style="color: #000000 !important; text-decoration: none !important; font-family: Verdana, Geneva, sans-serif; font-size: 14px;">+506 7286-0858</span></a>
                      </td>
                    </tr>
                    <tr>
                      <td width="20" style="text-align: center; vertical-align: middle; font-family: Verdana, Geneva, sans-serif;">
                        <img src="https://www.conofamily.org/wp-content/uploads/2026/07/mail_24dp_1D5EA0_FILL0_wght400_GRAD0_opsz24.png" width="16" height="16" alt="Email" style="display: block; margin: 0 auto; border: 0;">
                      </td>
                      <td style="padding: 2px 0 2px 8px; vertical-align: middle; font-family: Verdana, Geneva, sans-serif; font-size: 14px; color: #000000;">
                        <a href="mailto:daniela.castro@rodcastsolutions.com" style="text-decoration: none; color: #000000; font-family: Verdana, Geneva, sans-serif; font-size: 14px;"><span style="color: #000000 !important; text-decoration: none !important; font-family: Verdana, Geneva, sans-serif; font-size: 14px;">daniela.castro@rodcastsolutions.com</span></a>
                      </td>
                    </tr>
                    <tr>
                      <td width="20" style="text-align: center; vertical-align: middle; font-family: Verdana, Geneva, sans-serif;">
                        <img src="https://www.conofamily.org/wp-content/uploads/2026/07/public_24dp_1D5EA0_FILL0_wght400_GRAD0_opsz24.png" width="16" height="16" alt="Web" style="display: block; margin: 0 auto; border: 0;">
                      </td>
                      <td style="padding: 2px 0 2px 8px; vertical-align: middle; font-family: Verdana, Geneva, sans-serif; font-size: 14px; color: #000000;">
                        <a href="https://www.rodcastsolutions.com" style="text-decoration: none; color: #000000; font-family: Verdana, Geneva, sans-serif; font-size: 14px;"><span style="color: #000000 !important; text-decoration: none !important; font-family: Verdana, Geneva, sans-serif; font-size: 14px;">www.rodcastsolutions.com</span></a>
                      </td>
                    </tr>
                  </table>
                </td>
              </tr>
            </table>
            """;
    }
}
