using Todo_DurandalBreezeJsSignalR.App_Start;

[assembly: WebActivator.PostApplicationStartMethod(
	typeof(BundleConfig), "Init")]

namespace Todo_DurandalBreezeJsSignalR.App_Start
{
    using System;
    using System.Web.Optimization;

    public static class BundleConfig
	{
		public static void Init()
		{
			var bundles = BundleTable.Bundles;

			bundles.IgnoreList.Clear();
			AddDefaultIgnorePatterns(bundles.IgnoreList);

			AddDefaultBundles(bundles);
		}

		private static void AddDefaultIgnorePatterns(IgnoreList ignoreList)
		{
			if (ignoreList == null)
			{
				throw new ArgumentNullException("ignoreList");
			}

			ignoreList.Ignore("*.intellisense.js");
			ignoreList.Ignore("*-vsdoc.js");
			ignoreList.Ignore("*.debug.js", OptimizationMode.WhenEnabled);
		}

		private static void AddDefaultBundles(BundleCollection bundles)
		{
			bundles.Add(
				new ScriptBundle("~/scripts/jsextlibs")
					.Include("~/Scripts/jquery-{version}.js")
					.Include("~/Scripts/jquery-migrate-{version}.js")
                    .Include("~/Scripts/jquery.signalR-{version}.js")
					.Include("~/Scripts/knockout-{version}.js")
					.Include("~/Scripts/json2.js")
					.Include("~/Scripts/sammy-{version}.js")
                    .Include("~/scripts/Q.js")
                    .Include("~/scripts/breeze.debug.js")
                    .Include("~/Scripts/toastr.js")
                    .Include("~/Scripts/bootstrap{version}.js")
                );

			bundles.Add(
				new StyleBundle("~/Content/css")
					.Include("~/Content/bootstrap.css")
					.Include("~/Content/bootstrap-responsive.css")
					.Include("~/Content/font-awesome.min.css")
					.Include("~/Content/toastr.min.css")
					.Include("~/Content/Site.css")
				);
		}
	}
}