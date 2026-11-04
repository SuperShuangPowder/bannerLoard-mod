<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output
    omit-xml-declaration="no"
    indent="yes" />

  <!-- Preserve all NPC character data unless a more specific rule matches. -->
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()" />
    </xsl:copy>
  </xsl:template>

  <!-- Replace only the battle-roster helmets of the two Imperial T5 ranged troops. -->
  <xsl:template match="/NPCCharacters/NPCCharacter[@id='imperial_palatine_guard' or @id='imperial_sergeant_crossbowman']/Equipments/EquipmentRoster/equipment[@slot='Head']">
    <equipment
      slot="Head"
      id="Item.empire_lord_helmet" />
  </xsl:template>
</xsl:stylesheet>
