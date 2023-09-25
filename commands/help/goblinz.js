const { SlashCommandBuilder } = require('discord.js');

module.exports = {
  data: new SlashCommandBuilder()
  .setName('goblinz')
  .setDescription('BOUAHHH'),
	async execute(interaction) {
    let out = "```\n"
    out += "/##(\n"
    out += "  .*                      .                                             #,   ###. (#\n"
    out += "   ###.                    (                        ,,             ##    ####    ##\n"
    out += "    *#### #                #                        #               *#/## ##   .##\n"
    out += "     .## ###*               (                      **              *#(/(#   /.##.\n"
    out += "       #(  *##.             #.                     #            .#/  ,#     ,##\n"
    out += "        ##    ###           .#                  ..#( .        #*   (#      ##.\n"
    out += "         ##    ####,         #,       ,######/.  *#.##,    #,    ##.     /#(\n"
    out += "          ##    ,#( (#       /# ,####/           ###( ###      ##.      ##\n"
    out += "           ##     ##   ((   ( ##           .######      (##  ##,      (#*\n"
    out += "         ## ##.    (#/    #/  (#####*    #########,       ,##(       ##\n"
    out += "       .##   /#/     ## ##    (####          ####  *##(  (#( ###   (#.\n"
    out += "      /#/  ##..##     /#/      *## /#(        /#(*.    ,##   ,#####(\n"
    out += "     /#*(##     ##.  ##        ######         ##(######(.   /(##(#, (,\n"
    out += "     /##.        ,####  #     .*(##########(*#((#     ##( .#, ###/\n"
    out += "                  #########(*,    .   ,#,    (###*  /######   (##\n"
    out += "                 ### ## *###,       ##(/#, .##  ## ##. ##*    ##\n"
    out += "               .#/   ##. #* (#(   (##   ####.   ###(   #     ##\n"
    out += "                       ##*#   /###(                  #     .##\n"
    out += "                         ##*             /##.     .#,     ,#(\n"
    out += "                           ##.         *####    /#/      ,#\n"
    out += "                            .##       ##. *#*(#######/\n"
    out += "                              (##########(,./.\n"
    out += "                                   ##\n"
    out += "                                 (#.\n"
    out += "                                #, \n"
    out += "```"
    return interaction.reply({ content: out, ephemeral: true });
	},
};