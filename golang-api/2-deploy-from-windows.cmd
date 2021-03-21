set -eo pipefail
set /P ARTIFACT_BUCKET=< bucket-name.txt
cd src
set GOOS=linux
set GOARCH=amd64
set CGO_ENABLED=0
go build -o main main.go
%USERPROFILE%\Go\bin\build-lambda-zip.exe -o main.zip main
cd ../
aws cloudformation package --template-file template.yml --s3-bucket %ARTIFACT_BUCKET% --output-template-file out.yml
aws cloudformation deploy --template-file out.yml --stack-name blank-go --capabilities CAPABILITY_NAMED_IAM