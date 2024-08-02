using System;
namespace MDLabs.SchematicBrowserNavigation.Schemas
{
	public enum UserInterfacePage
	{
		Signup,
		Login,

		Home,
		Catalog,
		Blog,
		AboutUs,

		OtherOrUnknown
	}

	public enum UserInterfaceElement
	{
		LoginMenu,
		SignupMenu,

		HomeMenu,
        CatalogMenu,
		BlogMenu,
		AboutUsMenu,

		EmailInput,
		PasswordInput,
		LoginButton,
	}

	public enum UserInterfaceIntentType
	{
		NavigateToUrl,
		NavigateToPage,

		ClickButton,
		FillInput,
		SelectDropdownOption,
		HoverElement,
		FocusElement,
		BlurElement,

		AssertElementAttached,
		AssertElementDetached,
		AssertElementVisible,
		AssertElementHidden,

		// Safeguard
		OtherOrUnknown,

		WaitForNetwork,
		WaitForSelector
	}

	public class UserInterfaceIntent
	{
		public UserInterfaceIntentType Intent { get; set; }
		public UserInterfaceElement? TargetElement { get; set; }
		public UserInterfacePage? TargetPage { get; set; }
		public string[]? Inputs { get; set; }
	}
}

