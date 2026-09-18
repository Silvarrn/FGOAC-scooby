using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace FGOLocalPlatform;

public partial class Sbanners : UserControl
{
	private readonly Dictionary<string, CheckBox> bannerOptions = new();

	public Sbanners()
	{
		InitializeComponent();
		bannerOptions["HideBanners"] = HideBanners;
		bannerOptions["LTE8008"] = LTE8008;
		bannerOptions["LTE8010"] = LTE8010;
		bannerOptions["LTE8011"] = LTE8011;
		bannerOptions["LTE8012"] = LTE8012;
		bannerOptions["LTE8013"] = LTE8013;
		bannerOptions["LTE8014"] = LTE8014;
		bannerOptions["LTE8015"] = LTE8015;
		bannerOptions["LTE8016"] = LTE8016;
		bannerOptions["LTE8017"] = LTE8017;
		bannerOptions["LTE8018"] = LTE8018;
		bannerOptions["LTE8019"] = LTE8019;
		bannerOptions["LTE8020"] = LTE8020;
		bannerOptions["LTE8021"] = LTE8021;
		bannerOptions["LTE8022"] = LTE8022;
		bannerOptions["LTE8023"] = LTE8023;
		bannerOptions["LTE8024"] = LTE8024;
		bannerOptions["LTE8025"] = LTE8025;
		bannerOptions["LTE8026"] = LTE8026;
		bannerOptions["LTE8027"] = LTE8027;
		bannerOptions["LTE8028"] = LTE8028;
		bannerOptions["LTE8029"] = LTE8029;
		bannerOptions["LTE8030"] = LTE8030;
		bannerOptions["LTE8031"] = LTE8031;
		bannerOptions["LTE8032"] = LTE8032;
		bannerOptions["LTE8033"] = LTE8033;
		bannerOptions["LTE8034"] = LTE8034;
		bannerOptions["LTE8035"] = LTE8035;
		bannerOptions["LTE8036"] = LTE8036;
		bannerOptions["LTE8037"] = LTE8037;
		bannerOptions["LTE8038"] = LTE8038;
		bannerOptions["LTE8039"] = LTE8039;
		bannerOptions["LTE8040"] = LTE8040;
		bannerOptions["LTE8041"] = LTE8041;
		bannerOptions["LTE8042"] = LTE8042;
		bannerOptions["LTE8043"] = LTE8043;
		bannerOptions["LTE8044"] = LTE8044;
		bannerOptions["LTE8045"] = LTE8045;
		bannerOptions["LTE8046"] = LTE8046;
		bannerOptions["LTE8047"] = LTE8047;
		bannerOptions["LTE8048"] = LTE8048;
		bannerOptions["LTE8049"] = LTE8049;
	}

	private void Option_OnChanged(object sender, RoutedEventArgs e)
	{
		StatusText.Text = "Banner settings updated.";
	}

	public void Save()
	{
		StatusText.Text = "Banner settings saved.";
	}

	private void Save_OnClick(object sender, RoutedEventArgs e)
	{
		Save();
	}

	private void DisableAll_OnClick(object sender, RoutedEventArgs e)
	{
		foreach (CheckBox checkBox in bannerOptions.Values)
		{
			if (checkBox != null)
			{
				checkBox.IsChecked = false;
			}
		}
		StatusText.Text = "All banners turned off.";
	}
}
