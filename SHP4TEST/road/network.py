import networkx as nx
import matplotlib.pyplot as plt

G=nx.read_shp('./county_road.shp') #Read shapefile as graph

pos = {xy: xy for xy in G.nodes()}

nx.draw_networkx_nodes(G,pos,node_size=10,node_color='r')

nx.draw_networkx_edges(G,pos,edge_color='b')

plt.show()