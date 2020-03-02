RED='\033[0;31m'
NC='\033[0m'  #No Color
GREEN='\033[0;32m'
LIGHT_GREEN='\033[1;32m'

cd .. 
cd tests 
echo -e "${LIGHT_GREEN}Restoring packages...${NC}"
dotnet restore
#Build
echo -e "${LIGHT_GREEN}Building FakeXrmEasy.Core across all configurations...${NC}"
dotnet build --configuration FAKE_XRM_EASY_2013
dotnet build --configuration FAKE_XRM_EASY_2015
dotnet build --configuration FAKE_XRM_EASY_2016
dotnet build --configuration FAKE_XRM_EASY_365

echo -e "${LIGHT_GREEN}Running tests across all configurations...${NC}"
dotnet test --configuration FAKE_XRM_EASY_2013
dotnet test --configuration FAKE_XRM_EASY_2015
dotnet test --configuration FAKE_XRM_EASY_2016
dotnet test --configuration FAKE_XRM_EASY_365

echo -e "${LIGHT_GREEN}Done :)${NC}"
