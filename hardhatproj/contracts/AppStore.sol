//SPDX-License-Identifier: UNLICENSED
pragma solidity ^0.8.10;

import "@openzeppelin/contracts/access/Ownable.sol";
import {ERC721URIStorage, ERC721} from "@openzeppelin/contracts/token/ERC721/extensions/ERC721URIStorage.sol";
import "@openzeppelin/contracts/token/ERC721/IERC721.sol";

import "./LicenseService.sol";

contract AppStore is Ownable{
    
    struct AppDetails{
        uint256 price;
        address vendor;
    }
    
    LicenseService public licenseService;
    
    mapping(uint256 => AppDetails) public apps;
    
    event PaymentFinalized(address from, address to, uint256 sum);
    event RefundFinalized(address from, address to, uint256 sum);

    constructor(address _licenseService) Ownable(msg.sender){
        licenseService = LicenseService(_licenseService);
    }
    
    /*
        - vendor addr should be sent to the contract when api calls it, it's the wallet addr of the app publisher
    */
    
    function exists(uint256 appId) public view returns (bool){
        return apps[appId].vendor != address(0);
    }
    
    function addApp(uint256 appId, uint256 price, address vendor) external onlyOwner{
        apps[appId] = AppDetails(price, vendor);
    }
    
    function deleteApp(uint256 appId) external onlyOwner{
        require(apps[appId].vendor != address(0), "App not found");
        delete apps[appId];
    }
    
    function updateApp(uint256 appId, uint256 price, address vendor) external onlyOwner{
        if(apps[appId].price != price){
            apps[appId].price = price;
        }
        
        if(apps[appId].vendor != vendor){
            apps[appId].vendor = vendor;
        }
    }

    function buyApp(uint256 appId) external payable{
        AppDetails memory app = apps[appId];
        require(app.vendor != address(0), "App not found");
        
        if(app.price > 0) {
            require(msg.value == app.price, "Incorrect payment");
            payable(app.vendor).transfer(msg.value);
            emit PaymentFinalized(msg.sender, app.vendor, app.price);
        }
    }
    
    function refundApp(uint256 appId, address originalBuyer) external payable{
        AppDetails memory app = apps[appId];
        require(app.vendor != address(0), "App not found");
        require(licenseService.hasLicense(originalBuyer, appId), "License doesn't exist");
        
        if(app.price > 0){
            require(msg.value == app.price, "Incorrect payment");
            payable(app.vendor).transfer(msg.value);
            licenseService.revokeLicense(originalBuyer, appId);
            
            emit RefundFinalized(msg.sender, originalBuyer, app.price);
        }
    }
}