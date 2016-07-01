#addin nuget:?package=Newtonsoft.Json&version=8.03

// Arguments
var target = Argument<string>("target", "Default");
var version = Argument<string>("targetversion", null);
var nugetapikey = Argument<string>("apikey", "");
var BASE_GITHUB_PATH = "https://github.com/NancyFx";
var WORKING_DIRECTORY = "Working";
var projectJsonFiles = GetFiles("./Working/**/project.json");

var SUB_PROJECTS = new List<string>{
      "Nancy",
      "Nancy.Bootstrappers.Autofac",
      "Nancy.Bootstrappers.Ninject",
      "Nancy.Bootstrappers.StructureMap",
      "Nancy.Bootstrappers.Unity",
      "Nancy.Bootstrappers.Windsor",
      "Nancy.Serialization.ProtBuf",
      "Nancy.Serialization.ServiceStack",
      "Nancy.Serialization.JsonNet"
  };

Task("Run-Ninject-Build")
  .Does(() =>
{
   CakeExecuteScript("./Working/Bootstrappers/Ninject/build.cake");
});

Task("fix-submodules")
.Description("Updates all sub project submodules to point at master")
.Does(() =>
{
  
});

Task("Get-Projects")
.Description("Creates the working directory and gets projects from GitHub")
.IsDependentOn("Clean")
.Does(() =>
{
  CreateDirectory(WORKING_DIRECTORY);
  LogInfo("Getting projects from github account: "+BASE_GITHUB_PATH);
  SUB_PROJECTS.ForEach(project => {
           LogInfo("Getting "+ project +" from github");
           StartProcess("git", new ProcessSettings {
         Arguments = string.Format("clone --recursive {0} {1}/{2}", GetProjectGitUrl(project), WORKING_DIRECTORY,project)
         });
  });
});

Task("Build-Projects")
.IsDependentOn("Get-Projects")
.Description("Builds all projects")
.Does(() =>
{
 
  SUB_PROJECTS.ForEach(project => {
      LogInfo("Building "+ project);
      CakeExecuteScript(GetProjectDirectory(project) + "/build.cake", new CakeSettings{ Arguments = new Dictionary<string, string>{{"target", target}}});   
  });
});

Task("Clean")
.Description("Cleans up (deletes!) the working directory")
.Does(() =>
{
  CleanDirectories("./"+WORKING_DIRECTORY);
});

Task("Prepare-Release")
  .Does(() =>
 {
   // Update version.
   UpdateProjectJsonVersion(version, projectJsonFiles);
 
   // Add
   foreach (var file in projectJsonFiles) 
   {
     StartProcess("git", new ProcessSettings {
       Arguments = string.Format("add {0}", file.FullPath)
     });
   }
 
   // Commit
   StartProcess("git", new ProcessSettings {
     Arguments = string.Format("commit -m \"Updated version to {0}\"", version)
   });
   // Tag
   StartProcess("git", new ProcessSettings {
     Arguments = string.Format("tag \"v{0}\"", version)
   });
 });


Task("Default")
    .IsDependentOn("Build-Projects");
    
	
RunTarget(target);

public string GetProjectGitUrl(string project)
{
  return string.Format("{0}/{1}",BASE_GITHUB_PATH ,project);
}

public string GetProjectDirectory(string project)
{
 
  return string.Format("./{0}/{1}",WORKING_DIRECTORY ,project);
}

public void UpdateProjectJsonVersion(string version, FilePathCollection filePaths)
{
   LogInfo("Setting version to "+ version);
  foreach (var file in filePaths) 
  {
    var project = Newtonsoft.Json.Linq.JObject.Parse(
      System.IO.File.ReadAllText(file.FullPath, Encoding.UTF8));

    project["version"].Replace(version);

    System.IO.File.WriteAllText(file.FullPath, project.ToString(), Encoding.UTF8);
  }
}

public void LogInfo(string message)
{
  Information(logAction=>logAction(message));
}