require("@nomicfoundation/hardhat-toolbox");

/** @type import('hardhat/config').HardhatUserConfig */
module.exports = {
  solidity: "0.8.28",
  networks: {
    sepolia: {
      // FIX ME - store this securely!!
      url: "https://sepolia.infura.io/v3/d0830a3002cf4e00a65cb05ce6b31cf8",
      accounts: ["2f4b8a02ed20707bb8ff762f719a15c05979955c026a2c9442bc4eec2638a9eb"]
    }
  }
};
