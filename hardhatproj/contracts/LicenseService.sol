//SPDX-License-Identifier: UNLICENSED
pragma solidity ^0.8.10;

import "@openzeppelin/contracts/access/Ownable.sol";
import {ERC721URIStorage, ERC721} from "@openzeppelin/contracts/token/ERC721/extensions/ERC721URIStorage.sol";
import "@openzeppelin/contracts/token/ERC721/IERC721.sol";

contract LicenseService is ERC721URIStorage, Ownable{
    
    // user wallet -> app id -> token id
    mapping(address => mapping(uint256 => uint256)) private _licenses;
    mapping(address => mapping(uint256 => bool)) private licenseValidity;

    uint256 nextToken = 1;

    event LicenseMinted(uint256 indexed licenseId);
    
    constructor() ERC721("AppLicense", "ALC") Ownable(msg.sender){}

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

    function hasLicense(address user, uint256 appId) public view returns (bool) {
        return _licenses[user][appId] != 0 && licenseValidity[user][appId];
    }

    function getLicense(address user, uint256 appId) public view returns (uint256){
        return _licenses[user][appId];
    }

    function mintLicense(address buyer, uint256 appId, string memory tokenURI) public returns (uint256){
        require(_licenses[buyer][appId] == 0, "License already exists");
        uint256 licenseId = nextToken++;

        _safeMint(buyer, licenseId);
        _setTokenURI(licenseId, tokenURI);
        _licenses[buyer][appId] = licenseId;
        licenseValidity[buyer][appId] = true;

        emit LicenseMinted(licenseId);

        return licenseId;
    }
    
    function revokeLicense(address user, uint256 appId) public{
        require(_licenses[user][appId] != 0, "License doesn't exist");
        licenseValidity[user][appId] = false;
    }
}