﻿using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PraticeManagement.Configuration;
using System.Drawing;

namespace PraticeManagement.Controls
{
    public partial class BrandingSettingsControl : UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Populate current settings.
            if (!IsPostBack)
            {
                hdnImagePath.Value = BrandingConfigurationManager.GetLogoImageUrl();
                tbTitle.Text = HttpUtility.HtmlDecode(BrandingConfigurationManager.GetCompanyTitle());
                btnSave.Attributes.Add("disabled", "disabled");
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(fuImagePath.PostedFile.FileName))
            {
                mlConfirmation.ClearMessage();
                mlConfirmation.ShowErrorMessage("File path is required.");
                return;
            }
            if (Page.IsValid)
            {
                try
                {
                    SaveCompanyLogoData();
                    mlConfirmation.ShowInfoMessage(string.Format(Resources.Messages.SavedDetailsConfirmation, "Company Logo"));
                }
                catch (Exception ex)
                {
                    mlConfirmation.ClearMessage();
                    CustomValidator cvc = new CustomValidator();
                    cvc.ErrorMessage = string.Concat("Error occured. Can't save the settings: " + ex.Message);
                    this.fuImagePath.Controls.Add(cvc);
                    cvc.IsValid = false;
                }
            }
            else
            {
                mlConfirmation.ClearMessage();
            }

        }

        private void SaveCompanyLogoData()
        {
            var title = tbTitle.Text.Trim();
            if (fuImagePath.HasFile)
            {
                var imagename = fuImagePath.FileName;
                var imagePath = fuImagePath.PostedFile.FileName;
                Byte[] imageData = fuImagePath.FileBytes;
                BrandingConfigurationManager.SaveCompanyLogoData(title, imagename, imagePath, imageData);
            }
            else
            {
                BrandingConfigurationManager.SaveCompanyLogoData(title, null, null, null);
            }

        }

        protected void cvImagePath_OnServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = true;
            if (fuImagePath.HasFile)
            {
                string extension = System.IO.Path.GetExtension(fuImagePath.FileName);
                if (!(extension.ToLowerInvariant() == ".jpg" || extension.ToLowerInvariant() == ".gif" || extension.ToLowerInvariant() == ".png"))
                {
                    args.IsValid = false;
                }
            }
        }

        protected void cvalidatorImagePath_OnServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = true;
            if (fuImagePath.HasFile)
            {
                string extension = System.IO.Path.GetExtension(fuImagePath.FileName);
                if (extension.ToLowerInvariant() == ".jpg" || extension.ToLowerInvariant() == ".gif" || extension.ToLowerInvariant() == ".png")
                {
                    Bitmap bmp = new Bitmap(fuImagePath.FileContent);
                    int Height = bmp.Height;
                    int Width = bmp.Width;
                    if (!(Width <= 500 && Height <= 60))
                    {
                        args.IsValid = false;
                    }
                }
            }
        }
    }
}

