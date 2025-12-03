<table id="chat-body">
    <tr >
        <td style="height:500px">
            <div id="chat-display"></div>
        </td>
    </tr>
    <tr >
        <td id="chat-form" valign="top">
            <table id="chat-form-table">
                <tr>
                    <td style="width:100%">
                        <label for="chat-message" >Message</label>
                    </td>
                    <td></td>
                </tr>
                <tr>
                    <td>
                        <input type='hidden' id='chat-id' value='{$chatId}' />
                        <input type='hidden' id='chat-uid' value='{$userId}' />
                        <input type="text" class='rounded-input' id="chat-message" maxlength="250"  />
                    </td>
                    <td>
                        <button id="chat-submit">Envoyer</button>
                    </td>
                </tr>
            </table>
        </td>
    </tr>
</table>