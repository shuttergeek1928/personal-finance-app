import React from "react";
import { View, Text } from "react-native";
import { cn } from "../../utils/cn";

interface CardProps {
  children: React.ReactNode;
  className?: string;
}

export const Card = ({ children, className }: CardProps) => {
  return (
    <View
      className={cn(
        "bg-white dark:bg-zinc-900 rounded-2xl border border-zinc-200 dark:border-zinc-800 shadow-sm",
        className
      )}
    >
      {children}
    </View>
  );
};

export const CardHeader = ({
  children,
  className,
}: {
  children: React.ReactNode;
  className?: string;
}) => (
  <View className={cn("p-4 border-b border-zinc-100 dark:border-zinc-800", className)}>
    {children}
  </View>
);

export const CardTitle = ({
  children,
  className,
}: {
  children: React.ReactNode;
  className?: string;
}) => (
  <Text className={cn("text-lg font-bold text-zinc-900 dark:text-zinc-100", className)}>
    {children}
  </Text>
);

export const CardDescription = ({
  children,
  className,
}: {
  children: React.ReactNode;
  className?: string;
}) => (
  <Text className={cn("text-sm text-zinc-500 dark:text-zinc-400", className)}>
    {children}
  </Text>
);

export const CardContent = ({
  children,
  className,
}: {
  children: React.ReactNode;
  className?: string;
}) => <View className={cn("p-4", className)}>{children}</View>;

export const CardFooter = ({
  children,
  className,
}: {
  children: React.ReactNode;
  className?: string;
}) => (
  <View className={cn("p-4 border-t border-zinc-100 dark:border-zinc-800", className)}>
    {children}
  </View>
);
