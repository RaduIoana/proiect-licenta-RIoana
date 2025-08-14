const hre = require("hardhat");

async function main() {
    const [deployer] = await hre.ethers.getSigners(); // Get deployer wallet

    console.log(`Deploying contract with account: ${deployer.address}`);

    const unlockTime = Math.floor(Date.now() / 1000) + 3600; // 1 hour from now

    // Deploy contract with unlockTime
    const BuyApp = await hre.ethers.getContractFactory("BuyApp");
    const buyapp = await BuyApp.deploy();

    await buyapp.waitForDeployment();

    console.log(`BuyApp deployed to: ${await buyapp.getAddress()}`);
}

main().catch((error) => {
    console.error(error);
    process.exitCode = 1;
});