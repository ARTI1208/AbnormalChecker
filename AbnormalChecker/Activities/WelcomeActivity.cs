using System.Collections.Generic;
using Android.App;
using Android.OS;
using OnBoardingLib.Code;

namespace AbnormalChecker.Activities
{
	[Activity
	(
		Label = "AbnormalCheckerOn",
		Theme = "@style/MainTheme",
		Icon = "@mipmap/icon",
		MainLauncher = false
	)]
	public class WelcomeActivity : OnBoardingActivity
	{
		protected override void OnFinishButtonPressed()
		{
			Finish();
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