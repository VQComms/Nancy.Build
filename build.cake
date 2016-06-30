// Arguments
var target = Argument<string>("target", "Default");

Task("Run-Ninject-Build")
  .Does(() =>
{
   CakeExecuteScript("./Working/Bootstrappers/Ninject/build.cake");
});

Task("Default")
    .IsDependentOn("Run-Ninject-Build");
	
RunTarget(target);