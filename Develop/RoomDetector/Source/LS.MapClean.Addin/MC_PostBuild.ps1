#####################################################################################################################
#####################################################################################################################
#
# Script for applying Post Build events for the LS.MapClean Addin project
#
#  In the Addin project in LS.MapClean.Addin, this script is executed with the following command:
#
#  PowerShell -File "$(ProjectDir)DH_PostBuild.ps1" $(ProjectDir) $(TargetDir)
#
#####################################################################################################################
#####################################################################################################################

###############
#  Functions  #
###############

# Function that creates the directories (in the arguments) if they don't exist
Function CreateDirectories
{
    If ($args -eq $null)
    {
        return
    }
    ElseIf ($args[0] -is [array])
    {
        Foreach ($Directory in $args[0])
        {
            if ( !(test-path -pathtype container $Directory)) 
            { 
               mkdir $Directory
            }
        }
    }
    echo " "
}

#################
#  Script Body  #
#################

$ProjectDir    = $args[0]
$BuildDir      = $args[1]

echo " "
echo "################################################################"
echo "Copying build output to %APPDATA%\Autodesk\ApplicationPlugins..."
echo "################################################################"
echo " "

# Create directory variables for the directories that we are going to copy files to.
$DHApplicationBundle = "LS-MapClean.bundle" # should match AutocadReg.RegistryVersion in ArxAppReg.cs
$TargetPath = (Get-Content env:APPDATA) + "\Autodesk\ApplicationPlugins\" + $DHApplicationBundle
$ContentsPath = $TargetPath + "\Contents"
$ResourcesPath = $ContentsPath + "\Resources"
$WindowsPath = $ContentsPath + "\Windows"
$Acad2012 = $WindowsPath + "\2012"
$zhCN = $Acad2012 + "\zh-CN"

# Check that the directories described above exist.  If not, create them.
CreateDirectories $TargetPath, $ContentsPath, $ResourcesPath, $WindowsPath, $Acad2012, $zhCN

# Copy xml files to $TargetPath
Copy-item -Force -Verbose ($ProjectDir + "\PackageContents.xml") $TargetPath

# Copy files to $PlatformPath
$excludeDLLs = @('AcCoreMgd.dll', 'AcCui*.dll', 'Acdbmgd.dll', 'Acmgd.dll', 'AcTcMgd.dll', 'AdWindows.dll')
Copy-item -Force -Verbose -Exclude $excludeDLLs ($BuildDir + "\*.dll") $Acad2012
Copy-item -Force -Verbose -Exclude $excludeDLLs ($BuildDir + "\*.pdb") $Acad2012
Copy-item -Force -Verbose ($BuildDir + "\*.dbx") $Acad2012
Copy-item -Force -Verbose ($BuildDir + "\*.config") $Acad2012
Copy-item -Force -Verbose ($BuildDir + "\zh-CN" + "\*.dll") $zhCN
#####################
#  End Script Body  #
#####################