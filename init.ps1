param($installPath, $toolsPath, $package)

if (Get-Module | ?{ $_.Name -eq "FluentMigratorNHibernateGenerator" })
{
    Remove-Module FluentMigratorNHibernateGenerator
}

Import-Module (Join-Path $toolsPath "FluentMigratorNHibernateGenerator.psd1")