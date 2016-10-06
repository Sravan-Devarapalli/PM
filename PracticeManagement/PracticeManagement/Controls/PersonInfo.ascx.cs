﻿
using System.ComponentModel;
using DataTransferObjects;

namespace PraticeManagement.Controls
{
	public partial class PersonInfo : System.Web.UI.UserControl
	{
		private Person personValue;

        public string FirstName
        {
            get
            {
                return lblPersonFirstName.Text;
            }
        }

        public string LastName
        {
            get
            {
                return lblPersonLastName.Text;
            }
        }

		[Localizable(false)]
		[Browsable(false)]
		[Bindable(true)]
		public Person Person
		{
			get
			{
				return personValue;
			}
			set
			{
				personValue = value;
				lblPersonFirstName.Text = personValue.FirstName;
				lblPersonLastName.Text = personValue.LastName;
			}
		}
	}
}

