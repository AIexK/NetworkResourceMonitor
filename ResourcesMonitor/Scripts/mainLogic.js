const RESULT_OK = 0;
const RESULT_COMMON_HANDLED_ERROR = -2;
var counter = 0;
var isFirstInit = true;
var lastUpdateTime;

$(function () {
    var notificationhub = $.connection.notificationHub;
    notificationhub.client.displayMessage = function (message) {
        parsedMessage = JSON.parse(message);
        if (parsedMessage.ErrorCode == RESULT_OK || parsedMessage.ErrorCode == RESULT_COMMON_HANDLED_ERROR) {
                    
            if (isFirstInit) {
                $(".outer-spinner").remove();
                // Creating blocks
                createBlocks(parsedMessage.Data);
                updateSyncLastTime();
                isFirstInit = false;
            }
            else {
                $(".update-spinner").css("display", "inline-block");                        
                // Update blocls
                updateBlocks(parsedMessage.Data);
                        
                for (var i = 0; i < parsedMessage.Data.ServersLogicGroupsDto.length; i++) {
                    var groupId = parsedMessage.Data.ServersLogicGroupsDto[i].Id;
                    var websiteStatesDto = parsedMessage.Data.ServersLogicGroupsDto[i].WebsiteStatesDto;
                    if (websiteStatesDto != null && websiteStatesDto.length != 0) {
                        makeSitesBlock(groupId, websiteStatesDto);
                    }
                }

                setTimeout(function () {
                    $(".update-spinner").css("display", "none");
                    updateSyncLastTime();
                }, 1500);
            }

            if (parsedMessage.ErrorCode == RESULT_OK) {
                var serverData = parsedMessage.Data.ServersStatusDto;
                for (var i = 0; i < serverData.length; i++) {
                    var serverId = serverData[i].Id;
                    $('#commonErrorText_' + serverId).css("display", "none");
                }
            }
            if (parsedMessage.ErrorCode == RESULT_COMMON_HANDLED_ERROR) {
                try {
                    var serverData = parsedMessage.Data.ServersStatusDto;
                    for (var i = 0; i < serverData.length; i++) {
                        var serverId = serverData[i].Id;
                        var errorText = serverData[i].ErrorMessage;
                        if (errorText != '') {
                            var errorHtml = '<div><div class="alert-triangle blinking1"><i class="fa fa-exclamation-triangle fa-3x" aria-hidden="true"></i> <div class="error-caption-text">ERROR</div></div><br><div class="error-text-wrapper">' + errorText + '</div></div>';
                            $("#" + serverId).find('#commonErrorText_' + serverId)[0].innerHTML = errorHtml;
                            $('#commonErrorText_' + serverId).css("display", "inline-block");
                        }
                    }
                }
                catch (ex) {
                    console.log(parsedMessage.ServerId + " Message: " + ex.message);
                }
            }                                    
        }
        else {
            alert(parsedMessage.Message);
        }              
    };
    $.connection.hub.start();
});

function updateSyncLastTime() {
    var time = new Date();
    lastUpdateTime = time;
    var hh = time.getHours();
    if (hh < 10) {
        hh = "0" + hh;
    }
    var mm = time.getMinutes();
    if (mm < 10) {
        mm = "0" + mm;
    }
    var ss = time.getSeconds();
    if (ss < 10) {
        ss = "0" + ss;
    }
    $("#lastUpdateTime").text("Last update " + hh + ":" + mm + ":" + ss);
}

function createBlocks(blocksData) {
    if (blocksData == null) {
        return;
    }
    // Creating Server Logic Groups
    for (var i = 0; i < blocksData.ServersLogicGroupsDto.length; i++) {
        var groupId = blocksData.ServersLogicGroupsDto[i].Id;
        var groupCaption = blocksData.ServersLogicGroupsDto[i].Caption;
        var websiteStatesDto = blocksData.ServersLogicGroupsDto[i].WebsiteStatesDto;
        makeServerLogicGroup(groupId, groupCaption, websiteStatesDto);
    }
    for (var j = 0; j < blocksData.ServersStatusDto.length; j++) {
        var serverId = blocksData.ServersStatusDto[j].Id;
        var currentGroupId = blocksData.ServersStatusDto[j].LogicGroupId;
        makeServerBlock(serverId, currentGroupId);
        // Data about disk's space
        var disksSpaces = blocksData.ServersStatusDto[j].DiskSpace;
        for (var diskIndex = 0; diskIndex < disksSpaces.length; diskIndex++) {
            var diskInfo = blocksData.ServersStatusDto[j].DiskSpace[diskIndex];

            // Adding disk
            var letterForId = diskInfo.Letter.replace(':', '');
            var disksHtml = $('<div id="disk_' + letterForId + '"><span id="letter' + diskIndex + '">' + diskInfo.Letter +
                '/</span><span id="size' + diskIndex + '" class="sizeChart">' + diskInfo.FullnessPercent + '%</span> <span  id="freeSpace' + diskIndex + '" >(' + diskInfo.FreeSpaceToDisplay + ') </span></div>');
            $("#" + serverId).children().find('.block-disk-sizes').append(disksHtml);
        }

        // Data about services
        var services = blocksData.ServersStatusDto[j].WindowsServicesStatuses;
        if (services.length == 0){
            $("#" + serverId).find(".services-info-wrapper").remove();
        }
        for (var serviceIndex = 0; serviceIndex < services.length; serviceIndex++) {
            var name = services[serviceIndex].Name;
            var state = services[serviceIndex].StateName;
                    
            var className = 'service-ok';
            if (state != 'Running') {
                className = 'blinking';
            }
            var serviceHtml = '<div><span id="serviceName' + serviceIndex + '">' + name + '</span> <span id="serviceState' + serviceIndex + '" class="' + className + '">' + state + '</span></div>';
            $("#" + serverId).children().find('.services-info-text').append(serviceHtml);              
        }
        // If number of services more than 5, than set block on the side
        if (services.length <= 5) {
            $('#' + serverId).find('.block-server-main-info').css("display", "block");
        }               

        // Fill data about DB
        var baseUnderMonitoring = blocksData.ServersStatusDto[j].IsSqlBaseUnderMonitoring;               
                
        if (!baseUnderMonitoring) {
            $("#" + serverId).find(".sql-info-wrapper").remove();
        }
        else {
            $("#" + serverId).find("#allBasesOkCaption").remove();
                    
            var baseStatesHtml = '';
            var baseProblems = blocksData.ServersStatusDto[j].SqlBasesWithProblems;
            for (var baseIndex = 0; baseIndex < baseProblems.length; baseIndex++) {
                baseStatesHtml += '<div class="baseError"><span><b>' + baseProblems[baseIndex].Name + '</b>: ' + baseProblems[baseIndex].Status + ' ❌</span></div>';
            }
            $("#" + serverId).children().find('.bases-info-text').append(baseStatesHtml);
            $("#" + serverId).children().find('.sql-info-logo').addClass('blinking1');                   
        }
    }              
    initCharts();
    updateBlocks(blocksData);
}

function updateBlocks(blocksData) {
    if (blocksData == null) {
        return;
    }
    for (var j = 0; j < blocksData.ServersStatusDto.length; j++) {
        var serverData = blocksData.ServersStatusDto[j];
        var serverId = serverData.Id;
        var pingText = "";
        if (serverData.Ping == 0) {
            pingText = "< 1";
        }
        else {
            pingText = serverData.Ping;
        }
        $("#" + serverId).children().find('.host-name-header')[0].innerText = serverData.HostName;
        $("#" + serverId).children().find('.ip-header')[0].innerText = 'IP: ' + serverData.Ip;
        $("#" + serverId).children().find('.ping-header')[0].innerText = 'Ping: ' + pingText + ' ms';

        // Update info 
        try {
            var disksSpaces = blocksData.ServersStatusDto[j].DiskSpace;
            for (var diskIndex = 0; diskIndex < disksSpaces.length; diskIndex++) {
                var diskInfo = blocksData.ServersStatusDto[j].DiskSpace[diskIndex];

                var currentDiskId = 'disk_' + diskInfo.Letter;
                var letterForId = currentDiskId.replace(':', '');
                if (diskInfo.IsOverflow){
                    $("#" + serverId).children().find('#' + letterForId).addClass('blinking');
                }
                else{
                    $("#" + serverId).children().find('#' + letterForId).removeClass('blinking');
                }
                       
                $("#" + serverId).children().find('#letter' + diskIndex)[0].innerText = diskInfo.Letter + '/';
                $("#" + serverId).children().find('#size' + diskIndex)[0].innerText = diskInfo.FullnessPercent + '%';
                $("#" + serverId).children().find('#freeSpace' + diskIndex)[0].innerText = '(' + diskInfo.FreeSpaceToDisplay + ')';
            }
            initCharts();
        }
        catch(ex){
            console.log(ex.message);
        }

        // Update information about services
        try{
            var services = blocksData.ServersStatusDto[j].WindowsServicesStatuses;
            if (services.length == 0) {
                $("#" + serverId).children().find('.services-info-wrapper').remove();
                $("#" + serverId).find(".services-info-wrapper").remove();
            }
            for (var serviceIndex = 0; serviceIndex < services.length; serviceIndex++) {
                var name = services[serviceIndex].Name;
                var state = services[serviceIndex].StateName;

                var className = 'service-ok';
                if (state != 'Running') {
                    className = 'blinking';
                }
                $("#" + serverId).children().find('#serviceName' + serviceIndex)[0].innerText = name;
                $("#" + serverId).children().find('#serviceState' + serviceIndex)[0].innerHTML = '<span id="serviceState' + serviceIndex + '" class="' + className + '">' + state + '</span>';
            }
        }
        catch (ex) {
            console.log(ex.message);
        }
        // Update information about DB
        try{
            var baseUnderMonitoring = blocksData.ServersStatusDto[j].IsSqlBaseUnderMonitoring;
            if (!baseUnderMonitoring) {
                $("#" + serverId).find(".sql-info-wrapper").remove();
            }
            else {
                $("#" + serverId).find("#allBasesOkCaption").remove();

                var baseStatesHtml = '';
                var baseProblems = blocksData.ServersStatusDto[j].SqlBasesWithProblems;
                if (baseProblems.length > 0) {
                    $("#" + serverId).find(".baseError").remove();
                    for (var baseIndex = 0; baseIndex < baseProblems.length; baseIndex++) {
                        baseStatesHtml += '<div class="baseError"><span><b>' + baseProblems[baseIndex].Name + '</b>: ' + baseProblems[baseIndex].Status + ' ❌</span></div>';
                    }
                    $("#" + serverId).children().find('.bases-info-text').append(baseStatesHtml);
                    $("#" + serverId).children().find('.sql-info-logo').addClass('blinking1');
                }
                else {
                    $("#" + serverId).find(".baseError").remove();
                    $("#" + serverId).children().find('.sql-info-logo').removeClass('blinking1');
                    var allOkHtml = '<div id="allBasesOkCaption"><span>All bases are available ✔️</span></div>';
                    $("#" + serverId).children().find('.bases-info-text').append(allOkHtml);
                }
            }
        }
        catch (ex) {
            console.log(ex.message);
        }
    }
}

function initCharts() {
    $(".sizeChart").progressPie(
        {
            color: function (percent) {
                return percent >= 95 ? "#FF2829" : "#00B900";
            }
        });
}

$(document).ready(function () {
    setInterval(function () {
        var currentTime = new Date();
        if (currentTime - lastUpdateTime > 1000 * 60 * 5) { // 5 minutes
            location.reload();
        }                
    }, 50000);     
});

function makeServerLogicGroup(groupId, groupCaption, websiteStatesDto) {
    var serversGroupBlock = '<div class="servers-logic-group">' +
        '        <div class="servers-logic-group-caption">' +
        groupCaption +
        '        </div>' +
        '        <div id="' + groupId + '">' +
        '        </div>' +
        '    </div>';

    $("#main-block-for-adding").append(serversGroupBlock);

    if (websiteStatesDto.length > 0) {
        makeSitesBlock(groupId, websiteStatesDto);
    }
}

function makeSitesBlock(groupId, websiteStatesDto) {
    // Adding info about web-sites
    if (websiteStatesDto != null && websiteStatesDto.length > 0) {
        for (var i = 0; i < websiteStatesDto.length; i++) {
            var isAvailable = websiteStatesDto[i].IsAvailable;
            var statusText = '<div class="site-status-text-online">Status: ONLINE</div>';
            if (!isAvailable) {
                statusText = '<div class="blinking">Status: ERROR</div>';
            }
            var websitesBlock = 
            ' <div class="block-website-main-info" id="' + websiteStatesDto[i].Id + '">'
            + '<div class="host-name-header">' + websiteStatesDto[i].Name + '</div>'
            + '<a target="_blank" rel="noopener noreferrer" href="' + websiteStatesDto[i].Url + '">' + websiteStatesDto[i].Url + '</a>'
            + '<br>'
            + statusText
            + '</div>';
            $("#" + websiteStatesDto[i].Id).remove();
            $("#" + groupId).prepend(websitesBlock);
        }
    }
}

function makeServerBlock(blockId, logicGroupId) {
    var serverBlock = '<div class="main-resources-block" id="' + blockId + '">' +
        '        <div class="block-server-main-info">' +
        '' +
        '            <div class="server-text-info">' +
        '                <div class="host-name-header">' +
        '                    Host Name' +
        '                </div>' +
        '                <div class="ip-header">' +
        '                    IP: 111.111.111.111' +
        '                </div>' +
        '                <div class="ping-header">' +
        '                    Ping 1 ms' +
        '                </div>' +
        '            </div>' +
        '' +
        '            <div class="disk-info-wrapper">' +
        '<div><div class="icons-awesome"><i class="fa fa-hdd-o fa-2x" aria-hidden="true"></i></div> Used space (Free space)</div>' +
        '' +
        '                <div class="block-disk-sizes">' +
        '                </div>' +
        '            </div>' +
        '        </div>' +
        '        <div class="sql-info-wrapper">' +                
        '            <div class="sql-info-header sql-info-logo">' +
        '               <div class="icons-awesome sql-info-logo"><i class="fa fa-database fa-2x" aria-hidden="true"></i></div>  SQL Bases' +
        '            </div>' +
        '            <div class="bases-info-text">' +
        '                <div id="allBasesOkCaption"><span>All bases available ✔️</span></div>' +
        '            </div>' +
        '        </div>      ' +
        '        <div class="services-info-wrapper">' +
        '            <div class="services-info-header">' +
        '              <div class="icons-awesome"><i class="fa fa-cogs fa-2x" aria-hidden="true"></i></div>  Windows Services' +
        '            </div>' +
        '            <div class="services-info-text">' +
        '            </div>' +
        '        </div>' +
        '        <div id="commonErrorText_' + blockId + '" class="common-error-text">' +
        '        </div>' +
        '    </div>';
    $("#" + logicGroupId ).append(serverBlock);
            
}
