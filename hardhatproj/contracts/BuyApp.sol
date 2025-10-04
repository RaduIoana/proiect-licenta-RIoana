//SPDX-License-Identifier: UNLICENSED
pragma solidity ^0.8.10;

import "@openzeppelin/contracts/access/Ownable.sol";
import {ERC721URIStorage, ERC721} from "@openzeppelin/contracts/token/ERC721/extensions/ERC721URIStorage.sol";
import "@openzeppelin/contracts/token/ERC721/IERC721.sol";

contract BuyApp is ERC721URIStorage, Ownable{
    
    struct AppDetails{
        uint256 price;
        address vendor;
    }
    
    uint256 nextToken;
    
    mapping(uint256 => AppDetails) public apps;
    // user wallet -> app id -> token id
    mapping(address => mapping(uint256 => uint256)) private _licenses;
    
    event PaymentFinalized(address from, address to, uint256 sum);
    event LicenseMinted(uint256 indexed licenseId);

    constructor() ERC721("AppLicense", "ALC") Ownable(msg.sender){}
    
    /*
        - vendor addr should be sent to the contract when api calls it, it's the wallet addr of the app publisher
    */
    
    
    //following 3 functions make nft soulbound
    function _update(address to, uint256 tokenId, address auth) internal override returns (address) {
        address from = _ownerOf(tokenId);
        require(from == address(0), "Soulbound");
        return super._update(to, tokenId, auth);
    }

    function approve(address to, uint256 tokenId) public virtual override(ERC721, IERC721) {
        revert("Soulbound");
    }

    function setApprovalForAll(address operator, bool approved) public virtual override(ERC721, IERC721) {
        revert("Soulbound");
    }
    
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

    function hasLicense(address user, uint256 appId) public view returns (bool) {
        return _licenses[user][appId] != 0;
    }
    
    function mintLicense(address buyer, uint256 appId, string memory tokenURI) public returns (uint256){
        require(_licenses[buyer][appId] == 0, "License already exists");
        uint256 licenseId = nextToken++;

        _safeMint(buyer, licenseId);
        _setTokenURI(licenseId, tokenURI);
        _licenses[buyer][appId] = licenseId;
        
        emit LicenseMinted(licenseId);

        return licenseId;
    }
}