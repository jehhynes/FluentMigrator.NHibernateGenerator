function Add-FM
{
    [CmdletBinding(DefaultParameterSetName = 'Name')]
    param (
        [parameter(Position = 0,
            Mandatory = $true)]
        [string] $Name,
        [string] $ProjectName)

    if ($ProjectName) {
        $project = Get-Project $ProjectName
        if ($project -is [array])
        {
            throw "More than one project '$ProjectName' was found. Please specify the full name of the one to use."
        }
    }
    else {
        $project = Get-Project
    }
    
	Build-Project $project

	$installPath = Get-ThisPackageInstallPath $project
    $toolsPath = Join-Path $installPath lib

	$utilityAssembly = [System.Reflection.Assembly]::LoadFrom((Join-Path $toolsPath FluentMigrator.NHibernateGenerator.dll))

	$localPath = $project.Properties.Item("LocalPath").Value.ToString()
	$binDirectory = ($project.ConfigurationManager.ActiveConfiguration.Properties | Where Name -match OutputPath).Value 
	$outputFileName = $project.Properties.Item("OutputFileName").Value.ToString()

	$targetPath = [System.IO.Path]::Combine($localPath, $binDirectory, $outputFileName)
	$assemblyName = $project.Properties.Item("AssemblyName").Value.ToString();

	$migration = [FluentMigrator.NHibernateGenerator.NugetTooling]::Generate($targetPath, $assemblyName, $Name)
	
	if(-not ($migration))
	{
		throw "Failed to generate migration"
	}

	$migrationsPath = Join-Path $localPath $migration.MigrationsDirectory
	$newFileName = $migration.FileNamePrefix + $migration.Name + ".cs"
	$newFileNameDesigner = $migration.FileNamePrefix + $migration.Name + ".Designer.cs"
    $outputPath = Join-Path $migrationsPath $newFileName
    $outputPathDesigner = Join-Path $migrationsPath $newFileNameDesigner

    if (-not (Test-Path $migrationsPath))
    {
        [System.IO.Directory]::CreateDirectory($migrationsPath)
    }

    $migration.Code | Out-File -Encoding "UTF8" -Force $outputPath
	$migration.Designer | Out-File -Encoding "UTF8" -Force $outputPathDesigner

    $trash = $project.ProjectItems.AddFromFile($outputPath)
    $trash = $project.ProjectItems.AddFromFile($outputPathDesigner)
    $project.Save($null)

	$DTE.ExecuteCommand("File.OpenFile", $outputPath)
}

function Update-FM
{
    [CmdletBinding(DefaultParameterSetName = 'Name')]
    param (
        [parameter(Position = 0,
            Mandatory = $true)]
        [string] $Name,
        [string] $ProjectName)

    if ($ProjectName) {
        $project = Get-Project $ProjectName
        if ($project -is [array])
        {
            throw "More than one project '$ProjectName' was found. Please specify the full name of the one to use."
        }
    }
    else {
        $project = Get-Project
    }
    
	Build-Project $project

	$installPath = Get-ThisPackageInstallPath $project
    $toolsPath = Join-Path $installPath lib

	$utilityAssembly = [System.Reflection.Assembly]::LoadFrom((Join-Path $toolsPath FluentMigrator.NHibernateGenerator.dll))

	$localPath = $project.Properties.Item("LocalPath").Value.ToString()
	$binDirectory = ($project.ConfigurationManager.ActiveConfiguration.Properties | Where Name -match OutputPath).Value 
	$outputFileName = $project.Properties.Item("OutputFileName").Value.ToString()

	$targetPath = [System.IO.Path]::Combine($localPath, $binDirectory, $outputFileName)
	$assemblyName = $project.Properties.Item("AssemblyName").Value.ToString();

	$migration = [FluentMigrator.NHibernateGenerator.NugetTooling]::Generate($targetPath, $assemblyName, $Name)
	
	$migrationsPath = Join-Path $localPath $migration.MigrationsDirectory
	$migrationFileNameEndsWith = $migration.Name + ".cs"
	$migrationDesignerFileNameEndsWith = $migration.Name + ".Designer.cs"
	$migrationsFolderItem = Find-Project-Item $project $migration.MigrationsDirectory
	$matchingMigrationFileItem = $migrationsFolderItem.ProjectItems | Where Name -like *$migrationFileNameEndsWith
	
	if (-not($matchingMigrationFileItem))
	{
		throw "Could not find migration file in migrations folder ending with $migrationFileNameEndsWith"
	}

	$matchingMigrationDesignerFileItem = $matchingMigrationFileItem.ProjectItems | Where Name -like *$migrationDesignerFileNameEndsWith

	if (-not($matchingMigrationDesignerFileItem))
	{
		throw "Could not find migration designer file in migrations folder ending with $migrationDesignerFileNameEndsWith"
	}

	$newFileNameDesigner = $matchingMigrationDesignerFileItem.Name
    $outputPathDesigner = Join-Path $migrationsPath $newFileNameDesigner

    if (-not (Test-Path $outputPathDesigner))
    {
        throw "Could not find migration file $outputPathDesigner"
    }
	else 
	{
		$migration.Designer | Out-File -Encoding "UTF8" -Force $outputPathDesigner

		$project.Save($null)

		Write-Host "Updated migration designer file $newFileNameDesigner with latest model data"
	}
}

function Find-Project-Item($project, $directory)
{
	$currentItem = $project;
	$parts = $directory.Split('\')
	foreach($folder in $parts)
	{
		if($currentItem)
		{
			$currentItem = $currentItem.ProjectItems | Where Name -like $folder
		}
	}
	
	return $currentItem
}

function Build-Project($project)
{
    $configuration = $DTE.Solution.SolutionBuild.ActiveConfiguration.Name

    $DTE.Solution.SolutionBuild.BuildProject($configuration, $project.UniqueName, $true)

    if ($DTE.Solution.SolutionBuild.LastBuildInfo)
    {
        $projectName = $project.Name

        throw "The project '$projectName' failed to build."
    }
}

function Get-ThisPackageInstallPath($project)
{
    $package = Get-Package -ProjectName $project.FullName | ?{ $_.Id -eq 'FluentMigratorNHibernateGenerator' }

    if (!$package)
    {
        $projectName = $project.Name

        throw "The FluentMigratorNHibernateGenerator package is not installed on project '$projectName'."
    }
    
    return Get-PackageInstallPath $package
}
    
function Get-PackageInstallPath($package)
{
    $componentModel = Get-VsComponentModel
    $packageInstallerServices = $componentModel.GetService([NuGet.VisualStudio.IVsPackageInstallerServices])

    $vsPackage = $packageInstallerServices.GetInstalledPackages() | ?{ $_.Id -eq $package.Id -and $_.Version -eq $package.Version }
    
    return $vsPackage.InstallPath
}

Export-ModuleMember @( 'Add-FM', 'Update-FM' )
