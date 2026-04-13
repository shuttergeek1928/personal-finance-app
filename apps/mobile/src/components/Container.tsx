import React from "react";
import { View, ScrollView } from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { cn } from "../utils/cn";

interface ContainerProps {
  children: React.ReactNode;
  scrollable?: boolean;
  className?: string;
  contentPadding?: boolean;
}

export const Container = ({
  children,
  scrollable = false,
  className,
  contentPadding = true,
}: ContainerProps) => {
  const insets = useSafeAreaInsets();

  const content = (
    <View
      className={cn(
        "flex-1 bg-zinc-50 dark:bg-black",
        contentPadding && "px-4 pt-4",
        className
      )}
      style={{
        paddingTop: contentPadding ? insets.top + 16 : 0,
        paddingBottom: insets.bottom + 16,
      }}
    >
      {children}
    </View>
  );

  if (scrollable) {
    return (
      <ScrollView
        className="flex-1 bg-zinc-50 dark:bg-black"
        contentContainerStyle={{ flexGrow: 1 }}
        showsVerticalScrollIndicator={false}
      >
        {content}
      </ScrollView>
    );
  }

  return content;
};
