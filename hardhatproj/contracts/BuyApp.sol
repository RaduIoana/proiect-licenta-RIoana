//SPDX-License-Identifier: UNLICENSED
pragma solidity ^0.8.10;

import "@openzeppelin/contracts/access/Ownable.sol";
import {ERC721URIStorage, ERC721} from "@openzeppelin/contracts/token/ERC721/extensions/ERC721URIStorage.sol";

contract BuyApp is ERC721URIStorage, Ownable{
    
    struct AppDetails{
        uint256 price;
        address vendor;
    }
    
    uint256 nextToken;
    
    mapping(uint256 => AppDetails) public apps;
    //mapping(uint256 => uint256) public appIds;
    
    event PaymentFinalized(address from, address to, uint256 sum);

    constructor() ERC721("AppLicense", "ALC") Ownable(msg.sender){}
    
    /*
        - see if can use caller addr as the buyer directly to cut on gas -> msg.sender
        - vendor addr should be sent to the contract when api calls it, it's the wallet addr of the app publisher
        - buy: take x eth from buyer, deposit in contract (?), wait until receive signal that it was sent successfully
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
        /*, string memory tokenURI*/
        AppDetails memory app = apps[appId];
        require(app.vendor != address(0), "App not found");
        
        if(app.price > 0) {
            require(msg.value == app.price, "Incorrect payment");
            payable(app.vendor).transfer(msg.value);
            emit PaymentFinalized(msg.sender, app.vendor, app.price);
        }
        
        // commented until implemented in other components
        //uint256 licenseId = nextToken++;
        //_safeMint(msg.sender, licenseId);
        //_setTokenURI(licenseId, tokenURI);
        //appIds[licenseId] = appId;
    }
}