using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using OnBoardingLib.Code;

namespace AbnormalChecker.Activities
{
	[Activity
	(
		Label = "AbnormalCheckerOn",
		Theme = "@style/WelcomeTheme",
		Icon = "@mipmap/icon",
		MainLauncher = false
	)]
	public class WelcomeActivity : OnBoardingActivity
	{
		
		
		
		protected override void OnFinishButtonPressed()
		{
			SetResult(Result.Ok);
			Finish();
		}

		protected override void OnSettingsButtonClicked()
		{
			Intent intent = new Intent(this, typeof(SettingsActivity));
			StartActivity(intent);
		}

		protected override void OnSkipButtonClicked()
		{
			SetResult(Result.Canceled);
			base.OnSkipButtonClicked();
		}

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			List<OnBoardingCard> cardsList = new List<OnBoardingCard>();
			string welcomeTitle = string.Format(GetString(Resource.String.onboarding_page1_title),
				GetString(Resource.String.ApplicationName));
			OnBoardingCard welcomeCard =
				new OnBoardingCard(welcomeTitle, GetString(Resource.String.onboarding_page1_summary));
			welcomeCard.SetImageResourceId(Resource.Drawable.onboarding_welcome);
			cardsList.Add(welcomeCard);
			SetFinishButtonTitle(Resource.String.onboarding_finish_title_start);
			SetSkipButtonTitle(Resource.String.onboarding_button_skip);
			SetSettingsButtonTitle(Resource.String.onboarding_button_settings);
			SetFinishButtonTitle(Resource.String.onboarding_finish_title_start);
			if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
			{
				OnBoardingCard permissionsCard = new OnBoardingCard(Resource.String.onboarding_page2_title,
					Resource.String.onboarding_page2_summary);
				permissionsCard.SetImageResourceId(Resource.Drawable.onboarding_permissions);
				cardsList.Add(permissionsCard);
				SetFinishButtonTitle(Resource.String.onboarding_finish_title_give_permissions);
			}
			SetGradientBackground();
			SetOnBoardPages(cardsList);
		}
	}
}