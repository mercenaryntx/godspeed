echo off
echo pdb > exclude.lst
echo xml >> exclude.lst
echo vshost >> exclude.lst
echo exclude.lst >> exclude.lst
xcopy D:\User\Projects\VS\GODspeed\Neurotoxin.Godspeed\Neurotoxin.Godspeed.Shell\bin\Release\* D:\User\Projects\VS\GODspeed\Neurotoxin.Godspeed\..\Install\BuildResult\ /E /Y /EXCLUDE:exclude.lst
del exclude.lst 
pause