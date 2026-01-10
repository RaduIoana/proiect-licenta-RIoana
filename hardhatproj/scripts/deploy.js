const hre = require("hardhat");

async function main() {
    const [deployer] = await hre.ethers.getSigners(); // Get deployer wallet

    console.log(`Deploying contract with account: ${deployer.address}`);
    
    const LicenseService = await hre.ethers.getContractFactory("LicenseService");
    const licenseservice = await LicenseService.deploy();
    await licenseservice.waitForDeployment();
    console.log(`LicenseService deployed to: ${await licenseservice.getAddress()}`);
    
    const AppStore = await hre.ethers.getContractFactory("AppStore");
    const appstore = await AppStore.deploy(
        await licenseservice.getAddress()
    );
    await appstore.waitForDeployment();
    console.log(`AppStore deployed to: ${await appstore.getAddress()}`);
}

main().catch((error) => {
    console.error(error);
    process.exitCode = 1;
});