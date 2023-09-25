const { SlashCommandBuilder } = require('discord.js');

module.exports = {
  data: new SlashCommandBuilder()
  .setName('help')
  .setDescription('Get help'),
	async execute(interaction) {
    return interaction.reply({ content: "It's a calendar", ephemeral: true });
	},
};