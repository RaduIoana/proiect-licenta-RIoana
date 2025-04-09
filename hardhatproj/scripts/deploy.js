const hre = require("hardhat");

async function main() {
    const [deployer] = await hre.ethers.getSigners(); // Get deployer wallet

    console.log(`Deploying contract with account: ${deployer.address}`);

    const unlockTime = Math.floor(Date.now() / 1000) + 3600; // 1 hour from now

    // Deploy contract with unlockTime
    const Lock = await hre.ethers.getContractFactory("Lock");
    const lock = await Lock.deploy(unlockTime, { value: hre.ethers.parseEther("0.1") });

    await lock.waitForDeployment();

    console.log(`Lock deployed to: ${await lock.getAddress()}`);
}

main().catch((error) => {
    console.error(error);
    process.exitCode = 1;
});